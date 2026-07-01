<?php
/**
 * PUT /results/update.php
 *
 * Upserts a single student-unit result and appends a row to tblgrade_audit
 * so that the full history of every grade change is preserved.
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

$decoded    = requireAuth();
$body       = json_decode(file_get_contents('php://input'), true);
$student_id = (int)($body['student_id'] ?? 0);
$unit_id    = (int)($body['unit_id']    ?? 0);
$result     = strtoupper(trim($body['result'] ?? ''));
$raw_mark   = isset($body['raw_mark']) ? (int)$body['raw_mark'] : null;

$validResults = ['NS', 'OU', 'U', 'NP', 'P', 'M', 'D'];

if (!$student_id || !$unit_id || !in_array($result, $validResults, true)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id, unit_id and valid result required']);
    exit();
}

try {
    $db = getDb();

    // Read the current grade (if any) before overwriting it
    $existing = $db->prepare('SELECT result, raw_mark FROM tblresults WHERE student_id = ? AND unit_id = ?');
    $existing->execute([$student_id, $unit_id]);
    $prev = $existing->fetch(); // false if no row yet

    $old_result   = $prev ? $prev['result']   : null;
    $old_raw_mark = $prev ? $prev['raw_mark'] : null;

    // Upsert the result
    $upsert = $db->prepare(
        'INSERT INTO tblresults (student_id, unit_id, result, raw_mark, updated_by)
         VALUES (?, ?, ?, ?, ?)
         ON DUPLICATE KEY UPDATE
           result     = VALUES(result),
           raw_mark   = VALUES(raw_mark),
           updated_by = VALUES(updated_by)'
    );
    $upsert->execute([$student_id, $unit_id, $result, $raw_mark, $decoded->sub]);

    // Append to the audit log (always, even if the value did not change,
    // so that every deliberate staff action is on record)
    $audit = $db->prepare(
        'INSERT INTO tblgrade_audit
           (student_id, unit_id, old_result, new_result, old_raw_mark, new_raw_mark, changed_by)
         VALUES (?, ?, ?, ?, ?, ?, ?)'
    );
    $audit->execute([
        $student_id,
        $unit_id,
        $old_result,
        $result,
        $old_raw_mark,
        $raw_mark,
        $decoded->sub,
    ]);

    echo json_encode(['success' => true, 'message' => 'Result saved']);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
