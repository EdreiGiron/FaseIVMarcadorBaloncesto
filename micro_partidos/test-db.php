<?php

require_once 'vendor/autoload.php';

use Illuminate\Database\Capsule\Manager as Capsule;

$capsule = new Capsule;

$capsule->addConnection([
    'driver' => 'mysql',
    'host' => '127.0.0.1',
    'port' => '3306',
    'database' => 'marcador_players',
    'username' => 'jugadores',
    'password' => 'admin',
    'charset' => 'utf8mb4',
    'prefix' => '',
]);

$capsule->setAsGlobal();
$capsule->bootEloquent();

try {
    $result = $capsule::select('SELECT 1 as test');
    echo "✅ Conexión exitosa a SQL Server\n";
    
    // Verificar si existe la tabla
    $tables = $capsule::select("SELECT name FROM sys.tables WHERE name = 'php_partidos'");
    if (empty($tables)) {
        echo "❌ Tabla php_partidos no existe\n";
    } else {
        echo "✅ Tabla php_partidos existe\n";
    }
    
} catch (Exception $e) {
    echo "❌ Error de conexión: " . $e->getMessage() . "\n";
}