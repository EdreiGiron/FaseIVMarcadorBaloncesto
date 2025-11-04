<?php

namespace App\Http\Controllers;

use App\Models\Player;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Facades\Http;

class PlayerController extends Controller
{
    public function index(Request $request)
    {
        $query = Player::query();
        
        if ($request->has('search')) {
            $query->where('nombre', 'like', '%' . $request->search . '%');
        }
        
        if ($request->has('posicion')) {
            $query->where('posicion', 'like', '%' . $request->posicion . '%');
        }
        
        if ($request->has('equipoId')) {
            $query->where('team_id', $request->equipoId);
        }
        
        return $query->get()->map(function($player) {
            $equipoNombre = '';
            if ($player->team_id) {
                try {
                    $response = Http::get("http://127.0.0.1:8081/api/equipos/{$player->team_id}");
                    if ($response->successful()) {
                        $equipo = $response->json();
                        $equipoNombre = $equipo['nombre'] ?? '';
                    }
                } catch (\Exception $e) {
                    // Si falla la llamada, dejar vacío
                }
            }
            
            return [
                'id' => $player->id,
                'nombre' => $player->nombre,
                'numero' => $player->numero,
                'posicion' => $player->posicion,
                'puntos' => $player->puntos ?? 0,
                'faltas' => $player->faltas ?? 0,
                'equipoId' => $player->team_id,
                'equipoNombre' => $equipoNombre,
                'photoUrl' => null
            ];
        });
    }
    
    public function paged(Request $request)
    {
        $page = $request->get('page', 1);
        $pageSize = $request->get('pageSize', 10);
        $sortBy = $request->get('sortBy', 'nombre');
        $sortDir = $request->get('sortDir', 'asc');
        
        $query = Player::query();
        
        if ($request->has('search')) {
            $query->where('nombre', 'like', '%' . $request->search . '%');
        }
        
        if ($request->has('posicion')) {
            $query->where('posicion', 'like', '%' . $request->posicion . '%');
        }
        
        if ($request->has('equipoId')) {
            $query->where('team_id', $request->equipoId);
        }
        
        $total = $query->count();
        $items = $query->orderBy($sortBy, $sortDir)
                      ->skip(($page - 1) * $pageSize)
                      ->take($pageSize)
                      ->get()
                      ->map(function($player) {
                          $equipoNombre = '';
                          if ($player->team_id) {
                              try {
                                  $response = Http::get("http://127.0.0.1:8081/api/equipos/{$player->team_id}");
                                  if ($response->successful()) {
                                      $equipo = $response->json();
                                      $equipoNombre = $equipo['nombre'] ?? '';
                                  }
                              } catch (\Exception $e) {
                                  // Si falla la llamada, dejar vacío
                              }
                          }
                          
                          return [
                              'id' => $player->id,
                              'nombre' => $player->nombre,
                              'numero' => $player->numero,
                              'posicion' => $player->posicion,
                              'puntos' => $player->puntos ?? 0,
                              'faltas' => $player->faltas ?? 0,
                              'equipoId' => $player->team_id,
                              'equipoNombre' => $equipoNombre,
                              'photoUrl' => null
                          ];
                      });
        
        return response()->json([
            'items' => $items,
            'totalItems' => $total,
            'page' => (int)$page,
            'pageSize' => (int)$pageSize
        ]);
    }

    public function store(Request $request)
    {
        if (empty($request->nombre)) {
            return response()->json(['message' => 'El nombre es requerido'], 422);
        }

        $player = Player::create([
            'nombre' => $request->nombre,
            'numero' => $request->numero,
            'posicion' => $request->posicion,
            'team_id' => $request->equipoId,
        ]);

        $equipoNombre = '';
        if ($player->team_id) {
            try {
                $response = Http::get("http://127.0.0.1:8081/api/equipos/{$player->team_id}");
                if ($response->successful()) {
                    $equipo = $response->json();
                    $equipoNombre = $equipo['nombre'] ?? '';
                }
            } catch (\Exception $e) {
                // Si falla la llamada, dejar vacío
            }
        }

        return response()->json([
            'id' => $player->id,
            'nombre' => $player->nombre,
            'numero' => $player->numero,
            'posicion' => $player->posicion,
            'puntos' => $player->puntos ?? 0,
            'faltas' => $player->faltas ?? 0,
            'equipoId' => $player->team_id,
            'equipoNombre' => $equipoNombre,
            'photoUrl' => null
        ]);
    }

    public function show(Player $player)
    {
        return $player;
    }

    public function update(Request $request, Player $player)
    {
        if ($request->hasFile('photo')) {
            if ($player->photo_url) {
                $oldPath = str_replace('/storage/players/', 'public/players/', $player->photo_url);
                Storage::delete($oldPath);
            }
            
            $photoPath = $request->file('photo')->store('public/players');
            $player->photo_url = '/storage/players/' . basename($photoPath);
        }

        $player->update([
            'nombre' => $request->nombre ?? $player->nombre,
            'numero' => $request->numero ?? $player->numero,
            'posicion' => $request->posicion ?? $player->posicion,
            'team_id' => $request->equipoId ?? $player->team_id,
            'photo_url' => $player->photo_url,
        ]);

        return $player;
    }

    public function destroy(Player $player)
    {
        if ($player->photo_url) {
            $photoPath = str_replace('/storage/players/', 'public/players/', $player->photo_url);
            Storage::delete($photoPath);
        }
        
        $player->delete();
        return response()->json(null, 204);
    }
}