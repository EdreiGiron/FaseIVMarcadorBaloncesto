<?php

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\TorneoController;

Route::get('/user', function (Request $request) {
    return $request->user();
})->middleware('auth:sanctum');

// Rutas de torneos
Route::prefix('torneos')->group(function () {
    Route::get('/', [TorneoController::class, 'index']);
    Route::post('/', [TorneoController::class, 'store']);
    Route::get('/{id}', [TorneoController::class, 'show']);
    Route::post('/{id}/generar-siguiente-ronda', [TorneoController::class, 'generarSiguienteRonda']);
    Route::post('/seed-demo', [TorneoController::class, 'seedDemo']);
});