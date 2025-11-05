<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('php_torneos', function (Blueprint $table) {
            $table->id();
            $table->string('nombre');
            $table->integer('temporada');
            $table->integer('best_of')->default(5);
            $table->enum('estado', ['Planificado', 'Activo', 'Finalizado'])->default('Planificado');
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('php_torneos');
    }
};