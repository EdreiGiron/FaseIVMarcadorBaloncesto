<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Validation\Rule;

class TeamUpdateRequest extends FormRequest
{
    public function authorize(): bool { return true; }

    public function rules(): array {
        $id = $this->route('team')->id ?? $this->route('id');

        return [
            'nombre'  => ['required','string','max:120',
                Rule::unique('teams')
                    ->ignore($id)
                    ->where(fn($q) => $q->where('ciudad', $this->input('ciudad'))),
            ],
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
