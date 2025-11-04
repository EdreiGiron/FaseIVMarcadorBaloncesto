<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Player extends Model
{
    use HasFactory;

    protected $fillable = [
        'nombre',
        'numero',
        'posicion',
        'team_id',
        'puntos',
        'faltas',
    ];
    
    protected $attributes = [
        'puntos' => 0,
        'faltas' => 0,
    ];
}
