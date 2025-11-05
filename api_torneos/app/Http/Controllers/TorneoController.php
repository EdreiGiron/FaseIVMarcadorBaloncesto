<?php

namespace App\Http\Controllers;

use App\Models\Torneo;
use App\Models\SeriePlayoff;
use App\Models\Partido;
use Illuminate\Http\Request;
use Illuminate\Http\JsonResponse;
use Carbon\Carbon;

class TorneoController extends Controller
{
    public function index(): JsonResponse
    {
        $torneos = Torneo::orderBy('created_at', 'desc')->get();
        return response()->json($torneos);
    }

    public function show($id): JsonResponse
    {
        $torneo = Torneo::find($id);
        if (!$torneo) {
            return response()->json(['error' => 'Torneo no encontrado'], 404);
        }
        return response()->json($torneo);
    }

    public function store(Request $request): JsonResponse
    {
        $request->validate([
            'nombre' => 'required|string|max:255',
            'temporada' => 'required|integer',
            'best_of' => 'integer|min:1',
            'equipo_ids_seed' => 'required|array|min:2'
        ]);

        $equipoIds = $request->equipo_ids_seed;
        
        // Validar que los equipos existen (llamada a microservicio de equipos)
        if (!$this->validarEquipos($equipoIds)) {
            return response()->json(['error' => 'Algún equipo no existe'], 400);
        }

        $torneo = Torneo::create([
            'nombre' => $request->nombre,
            'temporada' => $request->temporada,
            'best_of' => $request->best_of ?: 5,
            'estado' => 'Planificado'
        ]);

        $this->generarRondaInicial($torneo->id, $equipoIds);
        
        $torneo->update(['estado' => 'Activo']);

        return response()->json($torneo, 201);
    }

    public function generarSiguienteRonda($id): JsonResponse
    {
        $torneo = Torneo::with(['series.partidos'])->find($id);
        if (!$torneo) {
            return response()->json(['error' => 'Torneo no encontrado'], 404);
        }

        // Verificar que no hay series abiertas
        if ($torneo->series->where('cerrada', false)->count() > 0) {
            return response()->json(['error' => 'Aún hay series abiertas'], 400);
        }

        $rondaMax = $torneo->series->max('ronda');
        
        // Si la última ronda fue Final, torneo finalizado
        if ($rondaMax === 'Final') {
            $torneo->update(['estado' => 'Finalizado']);
            return response()->json(['message' => 'Torneo finalizado']);
        }

        // Obtener ganadores y crear nueva ronda
        $ganadores = $torneo->series
            ->where('ronda', $rondaMax)
            ->sortBy('seed_a')
            ->map(function ($serie) {
                return $serie->ganador_equipo_id ?? 
                       ($serie->wins_a > $serie->wins_b ? $serie->equipo_a_id : $serie->equipo_b_id);
            })
            ->values()
            ->toArray();

        $nuevaRonda = $this->obtenerSiguienteRonda($rondaMax);
        $this->crearSeries($torneo->id, $nuevaRonda, $ganadores);

        return response()->json(['message' => 'Siguiente ronda generada']);
    }

    public function seedDemo(): JsonResponse
    {
        // Obtener equipos del microservicio de equipos
        $equipos = $this->obtenerEquipos();
        if (count($equipos) < 4) {
            return response()->json(['error' => 'Se requieren al menos 4 equipos'], 400);
        }

        $equipoIds = array_slice(array_column($equipos, 'id'), 0, 4);

        $torneo = Torneo::create([
            'nombre' => 'Playoffs Apertura (Demo)',
            'temporada' => date('Y'),
            'best_of' => 5,
            'estado' => 'Planificado'
        ]);

        $this->generarRondaInicial($torneo->id, $equipoIds);
        $torneo->update(['estado' => 'Activo']);

        // Ajustar algunos partidos para demo
        $this->ajustarPartidosDemo($torneo->id);

        return response()->json([
            'message' => 'Seed demo creado',
            'torneo_id' => $torneo->id
        ]);
    }

    private function validarEquipos(array $equipoIds): bool
    {
        // Aquí harías una llamada HTTP al microservicio de equipos
        // Por ahora retornamos true
        return true;
    }

    private function obtenerEquipos(): array
    {
        // Aquí harías una llamada HTTP al microservicio de equipos
        // Por ahora retornamos datos mock
        return [
            ['id' => 1, 'nombre' => 'Equipo 1'],
            ['id' => 2, 'nombre' => 'Equipo 2'],
            ['id' => 3, 'nombre' => 'Equipo 3'],
            ['id' => 4, 'nombre' => 'Equipo 4']
        ];
    }

    private function generarRondaInicial(int $torneoId, array $equipoIds): void
    {
        $n = count($equipoIds);
        $ronda = match ($n) {
            2 => 'Final',
            4 => 'Semifinal',
            8 => 'Cuartos',
            16 => 'Octavos',
            default => throw new \InvalidArgumentException('Cantidad de equipos no válida')
        };

        $this->crearSeries($torneoId, $ronda, $equipoIds);
    }

    private function crearSeries(int $torneoId, string $ronda, array $equipoIds): void
    {
        $n = count($equipoIds);
        $series = [];

        for ($i = 0; $i < $n / 2; $i++) {
            $series[] = SeriePlayoff::create([
                'torneo_id' => $torneoId,
                'ronda' => $ronda,
                'seed_a' => $i + 1,
                'seed_b' => $n - $i,
                'equipo_a_id' => $equipoIds[$i],
                'equipo_b_id' => $equipoIds[$n - 1 - $i],
                'best_of' => 0,
                'wins_a' => 0,
                'wins_b' => 0,
                'cerrada' => false
            ]);
        }

        // Programar Juego 1 de cada serie
        $fechaBase = Carbon::now()->addDay()->setHour(19)->setMinute(0);
        foreach ($series as $index => $serie) {
            Partido::create([
                'torneo_id' => $torneoId,
                'serie_playoff_id' => $serie->id,
                'game_number' => 1,
                'fecha_hora' => $fechaBase->copy()->addDays($index),
                'estado' => 'Programado',
                'equipo_local_id' => $serie->equipo_a_id,
                'equipo_visitante_id' => $serie->equipo_b_id
            ]);
        }
    }

    private function obtenerSiguienteRonda(string $rondaActual): string
    {
        return match ($rondaActual) {
            'Octavos' => 'Cuartos',
            'Cuartos' => 'Semifinal',
            'Semifinal' => 'Final',
            default => throw new \InvalidArgumentException('Ronda no válida')
        };
    }

    private function ajustarPartidosDemo(int $torneoId): void
    {
        $partidos = Partido::where('torneo_id', $torneoId)->orderBy('id')->take(3)->get();
        
        if ($partidos->count() >= 3) {
            // Partido pasado (finalizado)
            $partidos[0]->update([
                'fecha_hora' => Carbon::now()->subDays(2)->setHour(19),
                'estado' => 'Finalizado',
                'marcador_local' => 82,
                'marcador_visitante' => 76
            ]);

            // Actualizar serie
            $serie = $partidos[0]->serie;
            if ($partidos[0]->equipo_local_id === $serie->equipo_a_id) {
                $serie->increment('wins_a');
            } else {
                $serie->increment('wins_b');
            }

            // Partido en juego
            $partidos[1]->update([
                'fecha_hora' => Carbon::now(),
                'estado' => 'EnJuego'
            ]);

            // Partido futuro
            $partidos[2]->update([
                'fecha_hora' => Carbon::now()->addDay()->setHour(19),
                'estado' => 'Programado'
            ]);
        }
    }
}