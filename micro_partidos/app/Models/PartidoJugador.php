<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class PartidoJugador extends Model
{
    use HasFactory;

    protected $table = 'php_partido_jugadores';

    protected $fillable = [
        'partido_id',
        'equipo_id',
        'jugador_id',
        'titular'
    ];

    protected $casts = [
        'partido_id' => 'integer',
        'equipo_id' => 'integer',
        'jugador_id' => 'integer',
        'titular' => 'boolean'
    ];

    public function partido()
    {
        return $this->belongsTo(Partido::class, 'partido_id');
    }
}