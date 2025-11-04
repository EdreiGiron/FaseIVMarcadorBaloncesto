<?php

use Illuminate\Support\Facades\Route;

Route::get('/', function () {
    return ['service' => 'players', 'status' => 'running'];
});

// Servir fotos de jugadores
Route::get('/storage/players/{filename}', function ($filename) {
    $path = storage_path('app/public/players/' . $filename);
    if (!file_exists($path)) {
        abort(404);
    }
    return response()->file($path);
});