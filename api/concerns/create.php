<?php
/**
 * POST /concerns/create.php
 *
 * Creates a new concern category.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body    = json_decode(file_get_contents('php://input'), true);
$concern = trim($body['concern'] ?? '');

if (!$concern) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'concern required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('INSERT INTO tblconcern (concern) VALUES (?)');
    $stmt->execute([$concern]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $db->lastInsertId()], 'message' => 'Concern created']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
