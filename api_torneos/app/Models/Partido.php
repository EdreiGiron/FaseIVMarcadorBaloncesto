<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Partido extends Model
{
    use HasFactory;

    protected $table = 'php_partidos_torneo';

    protected $fillable = [
        'torneo_id',
        'serie_playoff_id',
        'game_number',
        'fecha_hora',
        'estado',
        'equipo_local_id',
        'equipo_visitante_id',
        'marcador_local',
        'marcador_visitante'
    ];

    protected $casts = [
        'torneo_id' => 'integer',
        'serie_playoff_id' => 'integer',
        'game_number' => 'integer',
        'fecha_hora' => 'datetime',
        'equipo_local_id' => 'integer',
        'equipo_visitante_id' => 'integer',
        'marcador_local' => 'integer',
        'marcador_visitante' => 'integer'
    ];

    public function torneo()
    {
        return $this->belongsTo(Torneo::class, 'torneo_id');
    }

    public function serie()
    {
        return $this->belongsTo(SeriePlayoff::class, 'serie_playoff_id');
    }
}