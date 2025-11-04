<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\TeamController;

Route::prefix('equipos')->group(function () {
    Route::get('/',        [TeamController::class, 'index']);   
    Route::get('/all',     [TeamController::class, 'index']);   
    Route::get('/paged',   [TeamController::class, 'paged']);    
    Route::post('/',       [TeamController::class, 'store']);   
    Route::get('/{team}',  [TeamController::class, 'show']);    
    Route::put('/{team}',  [TeamController::class, 'update']);   
    Route::delete('/{team}', [TeamController::class, 'destroy']); 
});
