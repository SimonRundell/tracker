<?php
/**
 * Configuration loader for AtRiskRegister API.
 *
 * Reads /api/.config.json and provides helper functions
 * for database connections and JWT configuration.
 *
 * @package    AtRiskRegister
 * @author     Exeter College
 * @license    CC NC-BY-SA 4.0
 */

$config = json_decode(
    file_get_contents(__DIR__ . '/.config.json'),
    true
);

/**
 * Returns a configured PDO database connection.
 *
 * @return PDO Active PDO connection in ERRMODE_EXCEPTION mode.
 * @throws PDOException On connection failure.
 */
function getDb(): PDO {
    global $config;
    $dsn = sprintf(
        'mysql:host=%s;dbname=%s;charset=utf8mb4',
        $config['db']['host'],
        $config['db']['name']
    );
    return new PDO($dsn, $config['db']['user'], $config['db']['pass'], [
        PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        PDO::ATTR_EMULATE_PREPARES   => false,
    ]);
}

/**
 * Returns JWT configuration values from .config.json.
 *
 * @return array{secret: string, accessExpiry: int, refreshExpiry: int}
 */
function getJwtConfig(): array {
    global $config;
    return $config['jwt'];
}
