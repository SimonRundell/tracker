<?php
/**
 * POST /criteria/create.php
 *
 * Creates a new criterion within a section.
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

$body        = json_decode(file_get_contents('php://input'), true);
$section_id  = (int)($body['section_id']  ?? 0);
$code        = trim($body['code']         ?? '');
$description = trim($body['description']  ?? '') ?: null;
$sort_order  = (int)($body['sort_order']  ?? 0);

if (!$section_id || !$code) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'section_id and code required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('INSERT INTO tblcriteria (section_id, code, description, sort_order) VALUES (?,?,?,?)');
    $stmt->execute([$section_id, $code, $description, $sort_order]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $db->lastInsertId()], 'message' => 'Criterion created']);
} catch (Exception $e) {
    if ($e->getCode() == 23000) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Criterion code already exists in this section']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
