<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;
use Illuminate\Database\Schema\Blueprint;

return new class extends Migration {
    public function up(): void
    {
        // Si no existe la tabla, no hay nada que renombrar
        if (!Schema::hasTable('players'))
            return;

        // Si la columna ya está en español, salir
        if (Schema::hasColumn('players', 'nombre'))
            return;

        // Solo si existe la columna antigua 'name', renombrar
        if (Schema::hasColumn('players', 'name')) {
            Schema::table('players', function (Blueprint $table) {
                $table->renameColumn('name', 'nombre'); // requiere doctrine/dbal en proyectos legacy
            });
        }

        // Crear columnas si no existen
        if (!Schema::hasColumn('players', 'numero')) {
            Schema::table('players', function (Blueprint $table) {
                $table->unsignedInteger('numero')->nullable();
            });
        }
        if (!Schema::hasColumn('players', 'posicion')) {
            Schema::table('players', function (Blueprint $table) {
                $table->string('posicion', 50)->nullable();
            });
        }
        if (!Schema::hasColumn('players', 'team_id')) {
            Schema::table('players', function (Blueprint $table) {
                $table->unsignedBigInteger('team_id')->nullable();
            });
        }
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
        if (!Schema::hasTable('players')) {
            return;
        }

        // Revertir solo si corresponde
        if (Schema::hasColumn('players', 'nombre') && !Schema::hasColumn('players', 'name')) {
            DB::statement('ALTER TABLE players RENAME COLUMN nombre TO name');
            // Alternativa:
            // DB::statement('ALTER TABLE players CHANGE COLUMN nombre name VARCHAR(255)');
        }

        // No elimino las columnas nuevas para no perder datos.
    }
};
