<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('php_partidos_torneo', function (Blueprint $table) {
            $table->id();
            $table->foreignId('torneo_id')->nullable()->constrained('php_torneos')->onDelete('cascade');
            $table->foreignId('serie_playoff_id')->nullable()->constrained('php_series_playoff')->onDelete('cascade');
            $table->integer('game_number')->default(1);
            $table->datetime('fecha_hora');
            $table->enum('estado', ['Programado', 'EnJuego', 'Finalizado', 'Pospuesto', 'Cancelado'])->default('Programado');
            $table->integer('equipo_local_id');
            $table->integer('equipo_visitante_id');
            $table->integer('marcador_local')->nullable();
            $table->integer('marcador_visitante')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('php_partidos_torneo');
    }
};