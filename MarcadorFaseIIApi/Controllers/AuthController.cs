﻿using Microsoft.AspNetCore.Mvc;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Services;
using MarcadorFaseIIApi.Services.Interfaces;
using MarcadorFaseIIApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace MarcadorFaseIIApi.Controllers;

/// <summary>
/// Endpoints de autenticación y gestión de sesión (login, refresh, registro, validación, logout).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Autentica un usuario con credenciales y devuelve tokens (access + refresh).
    /// </summary>
    /// <param name="request">DTO con <c>Username</c> y <c>Password</c>.</param>
    /// <returns>200 con <see cref="LoginResponseDto"/>; 401 si credenciales inválidas; 400 si modelo inválido.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var response = await _authService.AuthenticateAsync(request);
        if (response == null) return Unauthorized("Invalid username or password");
        return Ok(response);
    }

    /// <summary>
    /// Renueva el token de acceso usando un refresh token vigente.
    /// </summary>
    /// <param name="dto">DTO con <c>RefreshToken</c>.</param>
    /// <returns>200 con tokens renovados; 401 si el refresh token es inválido/expirado; 400 si modelo inválido.</returns>
    // ⬇ Hacemos refresh asincrónico y delegamos todo al servicio
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var tokens = await _authService.RefreshAsync(dto);
        if (tokens == null) return Unauthorized();
        return Ok(tokens);
    }

    /// <summary>
    /// Registra un nuevo usuario con rol asignado.
    /// </summary>
    /// <param name="request">DTO con <c>Username</c>, <c>Password</c> y <c>RoleId</c>.</param>
    /// <returns>200 con datos del usuario; 400 si ya existe o rol inválido.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Registro([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _authService.RegisterAsync(request);
        if (result == null)
            return BadRequest(new { message = "No se pudo registrar el usuario. Verifica el rol o si el usuario ya existe." });
        return Ok(result);
    }

    /// <summary>
    /// Valida que el access token actual sea aceptado por el middleware.
    /// </summary>
    /// <returns>200 con <c>{ valid = true }</c> si el token es válido.</returns>
    [HttpGet("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        return Ok(new { valid = true });
    }

    /// <summary>
    /// Finaliza la sesión a nivel servidor (no invalida refresh tokens por defecto).
    /// </summary>
    /// <returns>200 con mensaje informativo.</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var username = User?.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return Unauthorized();

        await _authService.LogoutAsync(username);
        return Ok(new { message = "Sesión cerrada correctamente." });
    }
}
