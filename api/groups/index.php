<?php
/**
 * GET /groups/index.php
 *
 * Returns all groups, optionally filtered by course_id.
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db        = getDb();
    $courseId  = isset($_GET['course_id']) ? (int)$_GET['course_id'] : null;

    if ($courseId) {
        $stmt = $db->prepare('SELECT id, course_id, groupname FROM tblgroup WHERE course_id = ? ORDER BY groupname');
        $stmt->execute([$courseId]);
    } else {
        $stmt = $db->query('SELECT id, course_id, groupname FROM tblgroup ORDER BY groupname');
    }

    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
