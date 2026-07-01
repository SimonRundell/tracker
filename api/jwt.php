<?php
/**
 * JWT utility functions for AtRiskRegister.
 *
 * Provides token generation and request authentication
 * using the firebase/php-jwt library.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/vendor/autoload.php';
require_once __DIR__ . '/config.php';

use Firebase\JWT\JWT;
use Firebase\JWT\Key;

/**
 * Generates a short-lived JWT access token.
 *
 * @param int    $userId The authenticated user's database ID.
 * @param string $email  The authenticated user's email address.
 * @return string Signed JWT string.
 */
function generateAccessToken(int $userId, string $email): string {
    $jwt = getJwtConfig();
    $payload = [
        'iss'   => 'atriskreg',
        'iat'   => time(),
        'exp'   => time() + $jwt['accessExpiry'],
        'sub'   => $userId,
        'email' => $email,
        'type'  => 'access',
    ];
    return JWT::encode($payload, $jwt['secret'], 'HS256');
}

/**
 * Generates a long-lived JWT refresh token.
 *
 * @param int $userId The authenticated user's database ID.
 * @return string Signed JWT string.
 */
function generateRefreshToken(int $userId): string {
    $jwt = getJwtConfig();
    $payload = [
        'iss'  => 'atriskreg',
        'iat'  => time(),
        'exp'  => time() + $jwt['refreshExpiry'],
        'sub'  => $userId,
        'type' => 'refresh',
    ];
    return JWT::encode($payload, $jwt['secret'], 'HS256');
}

/**
 * Validates the Bearer token in the Authorization header.
 *
 * Exits with HTTP 401 and a JSON error body if no token is
 * present or if the token is invalid/expired.
 *
 * @return object Decoded JWT payload (stdClass).
 */
function requireAuth(): object {
    $jwt     = getJwtConfig();
    $headers = getallheaders();
    $auth    = $headers['Authorization']
            ?? $headers['authorization']
            ?? $_SERVER['HTTP_AUTHORIZATION']
            ?? $_SERVER['REDIRECT_HTTP_AUTHORIZATION']
            ?? '';

    if (!preg_match('/^Bearer\s+(.+)$/', $auth, $matches)) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'No token provided']);
        exit();
    }

    try {
        return JWT::decode($matches[1], new Key($jwt['secret'], 'HS256'));
    } catch (Exception $e) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'Invalid or expired token']);
        exit();
    }
}
