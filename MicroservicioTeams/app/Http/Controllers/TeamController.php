<?php

namespace App\Http\Controllers;
/**Funciones generales controller Equipos-Teams
 *   - Gestión de equipos (CRUD).
 *   - Listado paginado y listado simple.
 *   - Manejo de logos (archivo), exponiéndolos por /storage/logos
 */
use App\Models\Team;
use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Storage;
use Illuminate\Validation\Rule;
use Illuminate\Database\QueryException;
use App\Http\Requests\TeamStoreRequest;
use App\Http\Requests\TeamUpdateRequest;

class TeamController extends Controller
{
    // Listado simple (array). Útil para otros microservicios.
    public function index(): JsonResponse
    {
        return response()->json(Team::orderBy('id', 'desc')->get());
    }

    // Listado paginado con nombres que el front espera
    public function paged(Request $request): JsonResponse
    {
        $page = max(1, (int) $request->get('page', 1));
        $pageSize = max(1, (int) $request->get('pageSize', 10));
        $sortBy = $request->get('sortBy', 'nombre');
        $sortDir = strtolower($request->get('sortDir', 'asc')) === 'desc' ? 'desc' : 'asc';

        // mapear alias del front → columnas reales
        $mapSort = [
            'name' => 'nombre',
            'city' => 'ciudad',
            'nombre' => 'nombre',
            'ciudad' => 'ciudad',
            'created_at' => 'created_at',
            'updated_at' => 'updated_at',
        ];
        $col = $mapSort[$sortBy] ?? 'nombre';

        $q = Team::orderBy($col, $sortDir)->paginate($pageSize, ['*'], 'page', $page);

        return response()->json([
            'items' => $q->items(),
            'totalItems' => $q->total(),
            'page' => $q->currentPage(),
            'pageSize' => $q->perPage(),
        ]);
    }

    public function store(Request $request)
    {
        try {
            // Detectar si viene en JSON puro y normalizar a claves esperadas
            if ($request->isJson() && empty($request->all())) {
                $json = $request->getContent();
                $data = json_decode($json, true) ?: [];
                $request->merge($data);
            }

            // Soportar mayúsculas usadas en algunos formularios
            if ($request->has('Nombre'))
                $request->merge(['nombre' => $request->input('Nombre')]);
            if ($request->has('Ciudad'))
                $request->merge(['ciudad' => $request->input('Ciudad')]);

            $validated = $request->validate([
                'nombre' => 'required|string|max:120',
                'ciudad' => 'required|string|max:120',
                'logo' => 'nullable|image|mimes:jpeg,png,jpg,gif|max:2048',
            ]);

            $data = [
                'nombre' => $validated['nombre'],
                'ciudad' => $validated['ciudad'],
            ];

            if ($request->hasFile('logo')) {
                $logoPath = $request->file('logo')->store('logos', 'public');
                $data['logo_url'] = url('storage/' . $logoPath);
            }

            $team = Team::create($data);
            return response()->json($team, 201);
        } catch (\Illuminate\Validation\ValidationException $e) {
            return response()->json([
                'message' => 'Validación fallida',
                'errors' => $e->errors(),
                'received' => $request->all(), // para depurar en UI
            ], 422);
        } catch (\Throwable $e) {
            return response()->json([
                'message' => 'Error de servidor',
                'error' => $e->getMessage(),
            ], 500);
        }
    }


    public function show(Team $team): JsonResponse
    {
        return response()->json($team);
    }

    public function update(Request $request, Team $team)
    {
        // Soportar mayúsculas usadas en algunos formularios
        if ($request->has('Nombre'))
            $request->merge(['nombre' => $request->input('Nombre')]);
        if ($request->has('Ciudad'))
            $request->merge(['ciudad' => $request->input('Ciudad')]);

        $validated = $request->validate([
            'nombre' => 'required|string|max:120',
            'ciudad' => 'required|string|max:120',
            'logo' => 'nullable|image|mimes:jpeg,png,jpg,gif|max:2048',
        ]);

        $data = [
            'nombre' => $validated['nombre'],
            'ciudad' => $validated['ciudad'],
        ];

        if ($request->hasFile('logo')) {
            if ($team->logo_url) {
                $old = str_replace(url('storage/'), '', $team->logo_url);
                Storage::disk('public')->delete($old);
            }
            $logoPath = $request->file('logo')->store('logos', 'public');
            $data['logo_url'] = url('storage/' . $logoPath);
        }

        $team->update($data);
        return response()->json($team, 200);
    }
    public function destroy(Team $team): JsonResponse
    {
        if ($team->logo_url) {
            $old = str_replace(url('storage/'), '', $team->logo_url);
            Storage::disk('public')->delete($old);
        }
        $team->delete();
        return response()->json(['deleted' => true]);
    }
}
