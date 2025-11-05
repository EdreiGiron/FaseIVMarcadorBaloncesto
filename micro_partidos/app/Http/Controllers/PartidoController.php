<?php

namespace App\Http\Controllers;

use App\Models\Partido;
use App\Models\PartidoJugador;
use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;
use Carbon\Carbon;
use Illuminate\Support\Facades\Http;

class PartidoController extends Controller
{
    public function index(Request $request): JsonResponse
    {
        $query = Partido::query();

        // Filtros
        if ($request->has('torneo_id')) {
            $query->where('torneo_id', $request->torneo_id);
        }

        if ($request->has('estado')) {
            $query->where('estado', $request->estado);
        }

        if ($request->has('equipo_id')) {
            $query->where(function ($q) use ($request) {
                $q->where('equipo_local_id', $request->equipo_id)
                  ->orWhere('equipo_visitante_id', $request->equipo_id);
            });
        }

        if ($request->has('fecha_desde')) {
            $query->where('fecha_hora', '>=', $request->fecha_desde);
        }

        if ($request->has('fecha_hasta')) {
            $query->where('fecha_hora', '<=', $request->fecha_hasta);
        }

        // Actualizar partidos programados que ya deberían estar en juego
        $this->actualizarEstadosPartidos();

        $partidos = $query->orderBy('fecha_hora', 'desc')->get();

        // Enriquecer con datos de equipos
        $partidosEnriquecidos = $this->enriquecerConEquipos($partidos);

        return response()->json($partidosEnriquecidos);
    }

    public function show($id): JsonResponse
    {
        $partido = Partido::find($id);
        if (!$partido) {
            return response()->json(['error' => 'Partido no encontrado'], 404);
        }

        $partidoEnriquecido = $this->enriquecerConEquipos(collect([$partido]))->first();
        return response()->json($partidoEnriquecido);
    }

    public function store(Request $request): JsonResponse
    {
        $request->validate([
            'fecha_hora' => 'required|date',
            'equipo_local_id' => 'required|integer',
            'equipo_visitante_id' => 'required|integer|different:equipo_local_id'
        ]);

        $partido = Partido::create([
            'fecha_hora' => $request->fecha_hora,
            'equipo_local_id' => $request->equipo_local_id,
            'equipo_visitante_id' => $request->equipo_visitante_id,
            'estado' => 'Programado'
        ]);

        return response()->json(['id' => $partido->id], 201);
    }

    public function cerrarPartido($id, Request $request): JsonResponse
    {
        $request->validate([
            'marcador_local' => 'required|integer|min:0',
            'marcador_visitante' => 'required|integer|min:0'
        ]);

        $partido = Partido::find($id);
        if (!$partido) {
            return response()->json(['error' => 'Partido no encontrado'], 404);
        }

        $partido->update([
            'marcador_local' => $request->marcador_local,
            'marcador_visitante' => $request->marcador_visitante,
            'estado' => 'Finalizado'
        ]);

        // Si es parte de una serie, actualizar la serie en el microservicio de torneos
        if ($partido->serie_playoff_id) {
            $this->actualizarSerie($partido);
        }

        return response()->json(['message' => 'Partido cerrado exitosamente']);
    }

    public function cambiarEstado($id, Request $request): JsonResponse
    {
        $request->validate([
            'estado' => 'required|in:Programado,EnJuego,Finalizado,Pospuesto,Cancelado'
        ]);

        $partido = Partido::find($id);
        if (!$partido) {
            return response()->json(['error' => 'Partido no encontrado'], 404);
        }

        $partido->update(['estado' => $request->estado]);

        return response()->json(['message' => 'Estado actualizado']);
    }

    public function asignarRoster($id, Request $request): JsonResponse
    {
        $request->validate([
            'equipo_id' => 'required|integer',
            'jugadores' => 'required|array|min:11|max:18',
            'jugadores.*.jugador_id' => 'required|integer',
            'jugadores.*.titular' => 'required|boolean'
        ]);

        $partido = Partido::find($id);
        if (!$partido) {
            return response()->json(['error' => 'Partido no encontrado'], 404);
        }

        // Validar que el equipo pertenece al partido
        if ($request->equipo_id != $partido->equipo_local_id && $request->equipo_id != $partido->equipo_visitante_id) {
            return response()->json(['error' => 'El equipo no participa en este partido'], 400);
        }

        // Validar jugadores únicos
        $jugadorIds = collect($request->jugadores)->pluck('jugador_id');
        if ($jugadorIds->count() !== $jugadorIds->unique()->count()) {
            return response()->json(['error' => 'No se pueden repetir jugadores en el roster'], 400);
        }

        // Validar que los jugadores existen y pertenecen al equipo
        $validacionJugadores = $this->validarJugadores($jugadorIds->toArray(), $request->equipo_id);
        if (!$validacionJugadores['valido']) {
            return response()->json(['error' => $validacionJugadores['mensaje']], 400);
        }

        // Contar titulares y suplentes
        $titulares = collect($request->jugadores)->where('titular', true);
        $suplentes = collect($request->jugadores)->where('titular', false);

        if ($titulares->count() != 11) {
            return response()->json(['error' => 'Debe haber exactamente 11 jugadores titulares'], 400);
        }

        if ($suplentes->count() > 7) {
            return response()->json(['error' => 'Máximo 7 jugadores suplentes permitidos'], 400);
        }

        // Eliminar roster anterior del equipo
        PartidoJugador::where('partido_id', $id)
                     ->where('equipo_id', $request->equipo_id)
                     ->delete();

        // Crear nuevo roster
        foreach ($request->jugadores as $jugador) {
            PartidoJugador::create([
                'partido_id' => $id,
                'equipo_id' => $request->equipo_id,
                'jugador_id' => $jugador['jugador_id'],
                'titular' => $jugador['titular']
            ]);
        }

        return response()->json([
            'message' => 'Roster asignado exitosamente',
            'titulares' => $titulares->count(),
            'suplentes' => $suplentes->count()
        ]);
    }

    public function getRoster($id, Request $request): JsonResponse
    {
        $partido = Partido::find($id);
        if (!$partido) {
            return response()->json(['error' => 'Partido no encontrado'], 404);
        }

        $query = PartidoJugador::where('partido_id', $id);
        
        // Filtrar por equipo si se especifica
        if ($request->has('equipo_id')) {
            $query->where('equipo_id', $request->equipo_id);
        }

        $roster = $query->orderBy('titular', 'desc')
                       ->orderBy('jugador_id')
                       ->get();
        
        // Enriquecer con datos de jugadores
        $rosterEnriquecido = $this->enriquecerConJugadores($roster);
        
        // Agrupar por equipo y tipo
        $rosterAgrupado = $rosterEnriquecido->groupBy('equipo_id')->map(function ($equipoRoster) {
            return [
                'titulares' => $equipoRoster->where('titular', true)->values(),
                'suplentes' => $equipoRoster->where('titular', false)->values()
            ];
        });

        return response()->json($rosterAgrupado);
    }

    public function historial(Request $request): JsonResponse
    {
        $query = Partido::query();

        // Aplicar filtros
        if ($request->has('torneo_id')) {
            $query->where('torneo_id', $request->torneo_id);
        }

        if ($request->has('estado')) {
            $query->where('estado', $request->estado);
        }

        if ($request->has('equipo_id')) {
            $query->where(function ($q) use ($request) {
                $q->where('equipo_local_id', $request->equipo_id)
                  ->orWhere('equipo_visitante_id', $request->equipo_id);
            });
        }

        if ($request->has('fecha_desde')) {
            $query->where('fecha_hora', '>=', $request->fecha_desde);
        }

        if ($request->has('fecha_hasta')) {
            $query->where('fecha_hora', '<=', $request->fecha_hasta);
        }

        // Paginación
        $page = $request->get('page', 1);
        $pageSize = $request->get('page_size', 10);
        
        $total = $query->count();
        $partidos = $query->orderBy('fecha_hora', 'desc')
                         ->skip(($page - 1) * $pageSize)
                         ->take($pageSize)
                         ->get();

        $partidosEnriquecidos = $this->enriquecerConEquipos($partidos);

        return response()->json([
            'items' => $partidosEnriquecidos,
            'total' => $total,
            'page' => $page,
            'page_size' => $pageSize
        ]);
    }

    private function actualizarEstadosPartidos(): void
    {
        $ahora = Carbon::now();
        
        Partido::where('estado', 'Programado')
               ->where('fecha_hora', '<=', $ahora)
               ->whereNull('marcador_local')
               ->whereNull('marcador_visitante')
               ->update(['estado' => 'EnJuego']);
    }

    private function enriquecerConEquipos($partidos)
    {
        if ($partidos->isEmpty()) {
            return collect([]);
        }

        $equipoIds = $partidos->pluck('equipo_local_id')
                             ->merge($partidos->pluck('equipo_visitante_id'))
                             ->unique()
                             ->values()
                             ->toArray();
        
        $equiposData = $this->obtenerDatosEquipos($equipoIds);

        return $partidos->map(function ($partido) use ($equiposData) {
            $equipoLocal = $equiposData[$partido->equipo_local_id] ?? null;
            $equipoVisitante = $equiposData[$partido->equipo_visitante_id] ?? null;
            
            return [
                'id' => $partido->id,
                'torneo_id' => $partido->torneo_id,
                'serie_playoff_id' => $partido->serie_playoff_id,
                'game_number' => $partido->game_number,
                'fecha_hora' => $partido->fecha_hora,
                'estado' => $partido->estado,
                'equipo_local_id' => $partido->equipo_local_id,
                'equipo_visitante_id' => $partido->equipo_visitante_id,
                'marcador_local' => $partido->marcador_local,
                'marcador_visitante' => $partido->marcador_visitante,
                'equipo_local_nombre' => $equipoLocal['nombre'] ?? 'Equipo ' . $partido->equipo_local_id,
                'equipo_visitante_nombre' => $equipoVisitante['nombre'] ?? 'Equipo ' . $partido->equipo_visitante_id,
                'equipo_local_logo' => $equipoLocal['logo_url'] ?? null,
                'equipo_visitante_logo' => $equipoVisitante['logo_url'] ?? null,
            ];
        });
    }

    private function obtenerDatosEquipos(array $equipoIds): array
    {
        try {
            $response = Http::timeout(5)->post(env('EQUIPOS_SERVICE_URL') . '/api/equipos/datos-partidos', [
                'equipo_ids' => $equipoIds
            ]);

            if ($response->successful()) {
                $equipos = $response->json();
                return collect($equipos)->keyBy('id')->toArray();
            }
        } catch (\Exception $e) {
            \Log::warning('Error obteniendo datos de equipos: ' . $e->getMessage());
        }

        return [];
    }

    private function enriquecerConJugadores($roster)
    {
        if ($roster->isEmpty()) {
            return collect([]);
        }

        $jugadorIds = $roster->pluck('jugador_id')->unique()->toArray();
        $jugadoresData = $this->obtenerDatosJugadores($jugadorIds);

        return $roster->map(function ($item) use ($jugadoresData) {
            $jugador = $jugadoresData[$item->jugador_id] ?? null;
            
            return [
                'id' => $item->id,
                'partido_id' => $item->partido_id,
                'equipo_id' => $item->equipo_id,
                'jugador_id' => $item->jugador_id,
                'jugador_nombre' => $jugador['nombre'] ?? 'Jugador ' . $item->jugador_id,
                'posicion' => $jugador['posicion'] ?? 'N/A',
                'dorsal' => $jugador['dorsal'] ?? null,
                'titular' => $item->titular,
                'created_at' => $item->created_at,
                'updated_at' => $item->updated_at
            ];
        });
    }

    private function validarJugadores(array $jugadorIds, int $equipoId): array
    {
        try {
            $response = Http::timeout(5)->post(env('JUGADORES_SERVICE_URL') . '/api/jugadores/validar-roster', [
                'jugador_ids' => $jugadorIds,
                'equipo_id' => $equipoId
            ]);

            if ($response->successful()) {
                $data = $response->json();
                return [
                    'valido' => $data['valido'] ?? false,
                    'mensaje' => $data['mensaje'] ?? 'Error en validación'
                ];
            }

            return ['valido' => false, 'mensaje' => 'Error al validar jugadores con API externa'];
        } catch (\Exception $e) {
            // Si falla la API, permitir el roster pero logear el error
            \Log::warning('Error validando jugadores: ' . $e->getMessage());
            return ['valido' => true, 'mensaje' => 'Validación omitida por error de conexión'];
        }
    }

    private function obtenerDatosJugadores(array $jugadorIds): array
    {
        try {
            $response = Http::timeout(5)->post(env('JUGADORES_SERVICE_URL') . '/api/jugadores/datos-roster', [
                'jugador_ids' => $jugadorIds
            ]);

            if ($response->successful()) {
                $jugadores = $response->json();
                return collect($jugadores)->keyBy('id')->toArray();
            }
        } catch (\Exception $e) {
            \Log::warning('Error obteniendo datos de jugadores: ' . $e->getMessage());
        }

        return [];
    }

    public function eliminarRoster($id, $equipoId): JsonResponse
    {
        $partido = Partido::find($id);
        if (!$partido) {
            return response()->json(['error' => 'Partido no encontrado'], 404);
        }

        $eliminados = PartidoJugador::where('partido_id', $id)
                                  ->where('equipo_id', $equipoId)
                                  ->delete();

        return response()->json([
            'message' => 'Roster eliminado',
            'jugadores_eliminados' => $eliminados
        ]);
    }

    public function cambiarTitularidad($id, $equipoId, $jugadorId, Request $request): JsonResponse
    {
        $request->validate([
            'titular' => 'required|boolean'
        ]);

        $partido = Partido::find($id);
        if (!$partido) {
            return response()->json(['error' => 'Partido no encontrado'], 404);
        }

        $partidoJugador = PartidoJugador::where('partido_id', $id)
                                       ->where('equipo_id', $equipoId)
                                       ->where('jugador_id', $jugadorId)
                                       ->first();

        if (!$partidoJugador) {
            return response()->json(['error' => 'Jugador no encontrado en el roster'], 404);
        }

        // Validar límites de titulares
        if ($request->titular) {
            $titularesActuales = PartidoJugador::where('partido_id', $id)
                                              ->where('equipo_id', $equipoId)
                                              ->where('titular', true)
                                              ->count();
            
            if ($titularesActuales >= 11 && !$partidoJugador->titular) {
                return response()->json(['error' => 'Ya hay 11 titulares asignados'], 400);
            }
        }

        $partidoJugador->update(['titular' => $request->titular]);

        return response()->json([
            'message' => 'Titularidad actualizada',
            'jugador_id' => $jugadorId,
            'titular' => $request->titular
        ]);
    }

    private function actualizarSerie($partido): void
    {
        // Aquí harías una llamada HTTP al microservicio de torneos
        // para actualizar la serie con el resultado del partido
    }
}