<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Validation\Rule;

class TeamStoreRequest extends FormRequest
{
    public function authorize(): bool { return true; }

    public function rules(): array {
        return [
            'nombre'  => ['required','string','max:120'],
            'ciudad'  => ['required','string','max:120'],
            'logo'    => ['nullable','image','mimes:jpeg,png,jpg,gif','max:2048'],
        ];
    }

    public function validated($key = null, $default = null) {
        $data = parent::validated();
        return [
            'nombre'   => $data['nombre'],
            'ciudad'   => $data['ciudad'],
            'logo'     => $data['logo'] ?? null,
        ];
    }
}
