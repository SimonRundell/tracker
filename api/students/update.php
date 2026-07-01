<?php
/**
 * PUT /students/update.php
 *
 * Updates a student's person-level fields (name, CIS number).
 * Enrollment and concern updates are handled by /enrollments/update.php.
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

$body      = json_decode(file_get_contents('php://input'), true);
$id        = (int)($body['id']        ?? 0);
$firstname = trim($body['firstname']  ?? '');
$lastname  = trim($body['lastname']   ?? '');
$cisnumber = trim($body['cisnumber']  ?? '') ?: null;

if (!$id || !$firstname || !$lastname) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'id, firstname and lastname required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE tblstudent SET firstname=?, lastname=?, cisnumber=? WHERE id=?');
    $stmt->execute([$firstname, $lastname, $cisnumber, $id]);

    echo json_encode(['success' => true, 'message' => 'Student updated']);
} catch (Exception $e) {
    if ($e->getCode() == 23000) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'CIS number already in use']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
