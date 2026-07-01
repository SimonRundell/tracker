<?php
/**
 * POST /enrollments/create.php
 *
 * Enrolls a student in a teaching group.
 * Returns 409 if the student is already enrolled in that group.
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

$body       = json_decode(file_get_contents('php://input'), true);
$student_id = (int)($body['student_id'] ?? 0);
$group_id   = (int)($body['group_id']   ?? 0);
$concern_id = isset($body['concern_id']) ? ((int)$body['concern_id'] ?: null) : null;

if (!$student_id || !$group_id) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id and group_id required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'INSERT INTO tblstudent_group (student_id, group_id, concern_id) VALUES (?, ?, ?)'
    );
    $stmt->execute([$student_id, $group_id, $concern_id]);

    http_response_code(201);
    echo json_encode([
        'success' => true,
        'data'    => ['id' => (int)$db->lastInsertId()],
        'message' => 'Student enrolled',
    ]);
} catch (Exception $e) {
    if ($e->getCode() === '23000') {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Student is already enrolled in this group']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
