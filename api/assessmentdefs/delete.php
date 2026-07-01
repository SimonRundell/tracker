<?php
/**
 * DELETE /assessmentdefs/delete.php
 *
 * Deletes an assessment definition.
 * Refuses if any student assessment records reference this definition.
 * Body: { id }
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';
require_once __DIR__ . '/../config.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'DELETE') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body = json_decode(file_get_contents('php://input'), true);
$id   = (int)($body['id'] ?? 0);

if (!$id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id required']);
    exit();
}

try {
    $db = getDb();

    $check = $db->prepare('SELECT COUNT(*) FROM tblassessment WHERE assessment_def_id = ?');
    $check->execute([$id]);
    if ((int)$check->fetchColumn() > 0) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Cannot delete: student assessment records exist for this definition']);
        exit();
    }

    $stmt = $db->prepare('DELETE FROM tblassessment_def WHERE id = ?');
    $stmt->execute([$id]);
    echo json_encode(['success' => true, 'message' => 'Assessment definition deleted']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
