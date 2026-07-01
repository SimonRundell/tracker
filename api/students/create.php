<?php
/**
 * POST /students/create.php
 *
 * Creates a new student (person record only).
 * Use /enrollments/create.php to assign the student to a group.
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

$body      = json_decode(file_get_contents('php://input'), true);
$firstname = trim($body['firstname'] ?? '');
$lastname  = trim($body['lastname']  ?? '');
$cisnumber = trim($body['cisnumber'] ?? '') ?: null;

if (!$firstname || !$lastname) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'firstname and lastname required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('INSERT INTO tblstudent (firstname, lastname, cisnumber) VALUES (?,?,?)');
    $stmt->execute([$firstname, $lastname, $cisnumber]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => (int)$db->lastInsertId()], 'message' => 'Student created']);
} catch (Exception $e) {
    if ($e->getCode() == 23000) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'CIS number already in use']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
