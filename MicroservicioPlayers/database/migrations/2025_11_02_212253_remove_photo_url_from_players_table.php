<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        // Evitar error si la tabla/columna no existen
        if (Schema::hasTable('players') && Schema::hasColumn('players', 'photo_url')) {
            Schema::table('players', function (Blueprint $table) {
                $table->dropColumn('photo_url');
            });
        }
        // Si no existe, no hacemos nada y la migración pasa sin fallar
    }

    public function down(): void
    {
        // Solo recrear si realmente no existe
        if (Schema::hasTable('players') && !Schema::hasColumn('players', 'photo_url')) {
            Schema::table('players', function (Blueprint $table) {
                $table->string('photo_url', 255)->nullable();
            });
        }
    }
};
