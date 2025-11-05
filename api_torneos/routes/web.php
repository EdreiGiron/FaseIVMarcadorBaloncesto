<?php

use Illuminate\Support\Facades\Route;

Route::get('/', function () {
    return ['service' => 'teams', 'status' => 'running'];
});

// Servir logos de equipos
Route::get('/storage/logos/{filename}', function ($filename) {
    $path = storage_path('app/public/logos/' . $filename);
    if (!file_exists($path)) {
        abort(404);
    }
    return response()->file($path);
});