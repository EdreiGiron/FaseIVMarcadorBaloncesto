<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Torneo extends Model
{
    use HasFactory;

    protected $table = 'php_torneos';

    protected $fillable = [
        'nombre',
        'temporada',
        'best_of',
        'estado'
    ];

    protected $casts = [
        'temporada' => 'integer',
        'best_of' => 'integer'
    ];

    public function series()
    {
        return $this->hasMany(SeriePlayoff::class, 'torneo_id');
    }

    public function partidos()
    {
        return $this->hasMany(Partido::class, 'torneo_id');
    }
}