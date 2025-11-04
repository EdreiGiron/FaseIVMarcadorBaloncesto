<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up()
    {
        Schema::table('players', function (Blueprint $table) {
            $table->renameColumn('name', 'nombre');
            $table->renameColumn('number', 'numero');
            $table->renameColumn('position', 'posicion');
        });
    }

    public function down()
    {
        Schema::table('players', function (Blueprint $table) {
            $table->renameColumn('nombre', 'name');
            $table->renameColumn('numero', 'number');
            $table->renameColumn('posicion', 'position');
        });
    }
};