<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Support\Facades\Schema;
use Illuminate\Database\Schema\Blueprint;

return new class extends Migration
{
    public function up(): void
    {
        // Agregar solo si NO existen
        if (!Schema::hasColumn('players', 'puntos')) {
            Schema::table('players', function (Blueprint $table) {
                $table->unsignedInteger('puntos')->default(0);
            });
        }

        if (!Schema::hasColumn('players', 'faltas')) {
            Schema::table('players', function (Blueprint $table) {
                $table->unsignedInteger('faltas')->default(0);
            });
        }
    }

    public function down(): void
    {
        // Quitar solo si existen
        if (Schema::hasColumn('players', 'faltas')) {
            Schema::table('players', function (Blueprint $table) {
                $table->dropColumn('faltas');
            });
        }
        if (Schema::hasColumn('players', 'puntos')) {
            Schema::table('players', function (Blueprint $table) {
                $table->dropColumn('puntos');
            });
        }
    }
};
