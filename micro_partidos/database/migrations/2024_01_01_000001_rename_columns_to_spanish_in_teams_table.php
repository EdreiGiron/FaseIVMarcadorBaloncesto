<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up()
    {
        Schema::table('teams', function (Blueprint $table) {
            $table->renameColumn('name', 'nombre');
            $table->renameColumn('city', 'ciudad');
        });
    }

    public function down()
    {
        Schema::table('teams', function (Blueprint $table) {
            $table->renameColumn('nombre', 'name');
            $table->renameColumn('ciudad', 'city');
        });
    }
};