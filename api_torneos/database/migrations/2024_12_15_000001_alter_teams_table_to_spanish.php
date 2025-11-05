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
        DB::statement('ALTER TABLE teams RENAME COLUMN name TO nombre');
        DB::statement('ALTER TABLE teams RENAME COLUMN city TO ciudad');
    }

    public function down()
    {
        DB::statement('ALTER TABLE teams RENAME COLUMN nombre TO name');
        DB::statement('ALTER TABLE teams RENAME COLUMN ciudad TO city');
    }
};