<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class SeriePlayoff extends Model
{
    use HasFactory;

    protected $table = 'php_series_playoff';

    protected $fillable = [
        'torneo_id',
        'ronda',
        'seed_a',
        'seed_b',
        'equipo_a_id',
        'equipo_b_id',
        'best_of',
        'wins_a',
        'wins_b',
        'cerrada',
        'ganador_equipo_id'
    ];

    protected $casts = [
        'torneo_id' => 'integer',
        'seed_a' => 'integer',
        'seed_b' => 'integer',
        'equipo_a_id' => 'integer',
        'equipo_b_id' => 'integer',
        'best_of' => 'integer',
        'wins_a' => 'integer',
        'wins_b' => 'integer',
        'cerrada' => 'boolean',
        'ganador_equipo_id' => 'integer'
    ];

    public function torneo()
    {
        return $this->belongsTo(Torneo::class, 'torneo_id');
    }

    public function partidos()
    {
        return $this->hasMany(Partido::class, 'serie_playoff_id');
    }
}