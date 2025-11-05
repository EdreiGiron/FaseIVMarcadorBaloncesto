<?php

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\PartidoController;
use App\Http\Controllers\TeamController;

Route::get('/user', function (Request $request) {
    return $request->user();
})->middleware('auth:sanctum');

// Rutas de partidos
Route::prefix('partidos')->group(function () {
    Route::get('/', [PartidoController::class, 'index']);
    Route::post('/', [PartidoController::class, 'store']);
    Route::get('/historial', [PartidoController::class, 'historial']);
    Route::get('/{id}', [PartidoController::class, 'show']);
    Route::put('/{id}/marcador', [PartidoController::class, 'cerrarPartido']);
    Route::put('/{id}/estado', [PartidoController::class, 'cambiarEstado']);
    
    // Rutas de roster
    Route::post('/{id}/roster', [PartidoController::class, 'asignarRoster']);
    Route::get('/{id}/roster', [PartidoController::class, 'getRoster']);
    Route::delete('/{id}/roster/{equipo_id}', [PartidoController::class, 'eliminarRoster']);
    Route::put('/{id}/roster/{equipo_id}/jugador/{jugador_id}', [PartidoController::class, 'cambiarTitularidad']);
});

// Rutas de equipos
Route::prefix('equipos')->group(function () {
    Route::get('/', [TeamController::class, 'index']);
    Route::get('/paginado', [TeamController::class, 'paged']);
    Route::post('/', [TeamController::class, 'store']);
    Route::get('/{team}', [TeamController::class, 'show']);
    Route::put('/{team}', [TeamController::class, 'update']);
    Route::delete('/{team}', [TeamController::class, 'destroy']);
});