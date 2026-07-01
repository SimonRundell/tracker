<?php
/**
 * PUT /students/notes.php
 *
 * Saves rich-text notes (HTML) against a student record.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'PUT') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body       = json_decode(file_get_contents('php://input'), true);
$student_id = (int)($body['student_id'] ?? 0);
$notes      = isset($body['notes']) ? (string)$body['notes'] : null;

if (!$student_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id required']);
    exit();
}

// Treat an empty string or whitespace-only value as NULL
if ($notes !== null && trim(strip_tags($notes)) === '') {
    $notes = null;
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tblstudent SET notes = ? WHERE id = ?');
    $stmt->execute([$notes, $student_id]);

    echo json_encode(['success' => true, 'message' => 'Notes saved']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
