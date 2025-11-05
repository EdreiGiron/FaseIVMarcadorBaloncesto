<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('php_partido_jugadores', function (Blueprint $table) {
            $table->id();
            $table->foreignId('partido_id')->constrained('php_partidos')->onDelete('cascade');
            $table->integer('equipo_id');
            $table->integer('jugador_id');
            $table->boolean('titular')->default(false);
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('php_partido_jugadores');
    }
};