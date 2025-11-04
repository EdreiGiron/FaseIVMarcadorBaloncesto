<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration
{
    public function up()
    {
        // Renombrar columnas usando SQL directo
        DB::statement('ALTER TABLE players RENAME COLUMN name TO nombre');
        DB::statement('ALTER TABLE players RENAME COLUMN number TO numero');
        DB::statement('ALTER TABLE players RENAME COLUMN position TO posicion');
    }

    public function down()
    {
        DB::statement('ALTER TABLE players RENAME COLUMN nombre TO name');
        DB::statement('ALTER TABLE players RENAME COLUMN numero TO number');
        DB::statement('ALTER TABLE players RENAME COLUMN posicion TO position');
    }
};