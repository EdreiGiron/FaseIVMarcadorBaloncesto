<?php

namespace App\Http\Controllers;

use App\Models\Team;
use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Storage;

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
        // Si viene JSON, merge a $request
        if ($request->isJson()) {
            $request->merge($request->json()->all());
        }

        // Mapear claves en inglés -> español
        $request->merge([
            'nombre' => $request->input('nombre', $request->input('name')),
            'ciudad' => $request->input('ciudad', $request->input('city')),
        ]);

        // Validación (logo opcional, máx 2MB)
        $data = validator($request->all(), [
            'nombre' => 'required|string|max:255',
            'ciudad' => 'nullable|string|max:255',
            'logo' => 'nullable|file|image|mimes:png,jpg,jpeg,webp|max:2048',
        ])->validate();

        $logoUrl = null;
        if ($request->hasFile('logo')) {
            $path = $request->file('logo')->store('public/logos');
            $logoUrl = '/storage/logos/' . basename($path);
        }

        $team = \App\Models\Team::create([
            'nombre' => $data['nombre'],
            'ciudad' => $data['ciudad'] ?? null,
            'logo_url' => $logoUrl,
        ]);

        return response()->json($team, 201);
    }


    public function show(Team $team): JsonResponse
    {
        return response()->json($team);
    }

    public function update(Request $request, Team $team)
    {
        if ($request->isJson()) {
            $request->merge($request->json()->all());
        }
        $request->merge([
            'nombre' => $request->input('nombre', $request->input('name', $team->nombre)),
            'ciudad' => $request->input('ciudad', $request->input('city', $team->ciudad)),
        ]);

        $data = validator($request->all(), [
            'nombre' => 'sometimes|required|string|max:255',
            'ciudad' => 'nullable|string|max:255',
            'logo' => 'nullable|file|image|mimes:png,jpg,jpeg,webp|max:2048',
        ])->validate();

        if ($request->hasFile('logo')) {
            if ($team->logo_url) {
                $old = str_replace('/storage/logos/', 'public/logos/', $team->logo_url);
                \Storage::delete($old);
            }
            $path = $request->file('logo')->store('public/logos');
            $team->logo_url = '/storage/logos/' . basename($path);
        }

        $team->fill([
            'nombre' => $data['nombre'] ?? $team->nombre,
            'ciudad' => $data['ciudad'] ?? $team->ciudad,
        ])->save();

        return response()->json($team);
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
