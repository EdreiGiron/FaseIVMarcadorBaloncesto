<?php

namespace App\Services;

use Illuminate\Support\Facades\Http;
use Illuminate\Support\Facades\Log;

class TorneosExternalService
{
    private string $equiposUrl;
    private string $partidosUrl;

    public function __construct()
    {
        $this->equiposUrl = config('services.equipos.url');
        $this->partidosUrl = config('services.partidos.url');
    }

    public function obtenerEquipos()
    {
        try {
            $response = Http::timeout(3)->get("{$this->equiposUrl}/api/equipos");
            return $response->successful() ? $response->json() : [];
        } catch (\Exception $e) {
            Log::warning("Error al obtener equipos: {$e->getMessage()}");
            return [];
        }
    }

    public function crearPartido(array $payload)
    {
        try {
            $response = Http::timeout(3)->post("{$this->partidosUrl}/api/partidos", $payload);
            if ($response->successful()) {
                return $response->json();
            }
            Log::warning("Error al crear partido: {$response->status()} {$response->body()}");
        } catch (\Exception $e) {
            Log::error("No se pudo crear partido: {$e->getMessage()}");
        }
        return null;
    }
}
