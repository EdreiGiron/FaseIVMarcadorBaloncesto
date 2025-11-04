<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\PlayerController;

Route::get('players/paged', [PlayerController::class, 'paged']);
Route::apiResource('players', PlayerController::class);

// Alias en español para compatibilidad
Route::get('jugadores', [PlayerController::class, 'index']);
Route::get('jugadores/paged', [PlayerController::class, 'paged']);