<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration
{
    public function up(): void
    {
        if (!Schema::hasTable('players')) return;

        // name -> nombre
        if (Schema::hasColumn('players', 'name') && !Schema::hasColumn('players', 'nombre')) {
            // MySQL 8+: no requiere tipos
            DB::statement('ALTER TABLE players RENAME COLUMN name TO nombre');
        }

        // number -> numero
        if (Schema::hasColumn('players', 'number') && !Schema::hasColumn('players', 'numero')) {
            DB::statement('ALTER TABLE players RENAME COLUMN number TO numero');
        }

        // position -> posicion
        if (Schema::hasColumn('players', 'position') && !Schema::hasColumn('players', 'posicion')) {
            DB::statement('ALTER TABLE players RENAME COLUMN position TO posicion');
        }
    }

    public function down(): void
    {
        if (!Schema::hasTable('players')) return;

        if (Schema::hasColumn('players', 'nombre') && !Schema::hasColumn('players', 'name')) {
            DB::statement('ALTER TABLE players RENAME COLUMN nombre TO name');
        }
        if (Schema::hasColumn('players', 'numero') && !Schema::hasColumn('players', 'number')) {
            DB::statement('ALTER TABLE players RENAME COLUMN numero TO number');
        }
        if (Schema::hasColumn('players', 'posicion') && !Schema::hasColumn('players', 'position')) {
            DB::statement('ALTER TABLE players RENAME COLUMN posicion TO position');
        }
    }
};
