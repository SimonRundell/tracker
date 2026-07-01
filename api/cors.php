<?php
/**
 * CORS and content-type header handler.
 *
 * Include at the very top of every API endpoint file,
 * before any output. Terminates immediately for OPTIONS
 * preflight requests.
 *
 * @package AtRiskRegister
 */

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: http://localhost:5173');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Authorization, Content-Type');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit();
}
