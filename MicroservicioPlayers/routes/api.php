<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\PlayerController;

// Rutas en inglés (original)
Route::get('players/paged', [PlayerController::class, 'paged']);
Route::apiResource('players', PlayerController::class)
    ->parameters(['players' => 'player'])
    ->names('players');

// Alias completo en español (incluye POST/PUT/DELETE)
Route::get('jugadores/paged', [PlayerController::class, 'paged']);
Route::apiResource('jugadores', PlayerController::class)
    ->parameters(['jugadores' => 'player'])
    ->names([
        'index'   => 'jugadores.index',
        'store'   => 'jugadores.store',
        'show'    => 'jugadores.show',
        'update'  => 'jugadores.update',
        'destroy' => 'jugadores.destroy',
    ]);
