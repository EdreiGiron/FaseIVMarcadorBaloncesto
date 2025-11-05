<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('php_series_playoff', function (Blueprint $table) {
            $table->id();
            $table->foreignId('torneo_id')->constrained('php_torneos')->onDelete('cascade');
            $table->enum('ronda', ['Final', 'Semifinal', 'Cuartos', 'Octavos']);
            $table->integer('seed_a');
            $table->integer('seed_b');
            $table->integer('equipo_a_id');
            $table->integer('equipo_b_id');
            $table->integer('best_of')->default(0);
            $table->integer('wins_a')->default(0);
            $table->integer('wins_b')->default(0);
            $table->boolean('cerrada')->default(false);
            $table->integer('ganador_equipo_id')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('php_series_playoff');
    }
};