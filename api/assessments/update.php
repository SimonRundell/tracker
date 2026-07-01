<?php
/**
 * PUT /assessments/update.php
 *
 * Batch-upserts assessment records for a single student.
 * Each item in the updates array is INSERTed or UPDATEd by the
 * unique key (student_id, assessment_def_id).
 *
 * Request body:
 *   {
 *     "student_id": 1,
 *     "updates": [
 *       {
 *         "assessment_def_id": 5,
 *         "status": "SET",
 *         "date_set": "2026-01-15",
 *         "date_deadline": "2026-02-01",
 *         "date_resubmission": null,
 *         "date_completed": null
 *       },
 *       ...
 *     ]
 *   }
 *
 * @package AtRiskRegister
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

$auth = requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'PUT') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body       = json_decode(file_get_contents('php://input'), true);
$student_id = (int)($body['student_id'] ?? 0);
$updates    = $body['updates'] ?? [];

if (!$student_id || !is_array($updates) || empty($updates)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'student_id and updates array required']);
    exit();
}

$valid_statuses = ['NOT_SET','SET','HANDED_IN_1','RETURNED','HANDED_IN_2','COMPLETE','INCOMPLETE'];

/** Returns a valid date string or NULL. */
function sanitiseDate(?string $val): ?string {
    if (!$val) return null;
    $d = \DateTime::createFromFormat('Y-m-d', $val);
    return ($d && $d->format('Y-m-d') === $val) ? $val : null;
}

try {
    $db      = getDb();
    $user_id = (int)($auth->sub ?? 0);

    $sql = 'INSERT INTO tblassessment
              (student_id, assessment_def_id, status, date_set, date_deadline, date_resubmission, date_completed, set_by_id, updated_by)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
            ON DUPLICATE KEY UPDATE
              status            = VALUES(status),
              date_set          = VALUES(date_set),
              date_deadline     = VALUES(date_deadline),
              date_resubmission = VALUES(date_resubmission),
              date_completed    = VALUES(date_completed),
              set_by_id         = CASE WHEN VALUES(date_set) IS NOT NULL THEN VALUES(set_by_id) ELSE set_by_id END,
              updated_by        = VALUES(updated_by),
              updated_at        = CURRENT_TIMESTAMP';

    $stmt = $db->prepare($sql);

    foreach ($updates as $row) {
        $def_id    = (int)($row['assessment_def_id'] ?? 0);
        $status    = in_array($row['status'] ?? '', $valid_statuses) ? $row['status'] : 'NOT_SET';
        $date_set  = sanitiseDate($row['date_set']          ?? null);
        $deadline  = sanitiseDate($row['date_deadline']     ?? null);
        $resub     = sanitiseDate($row['date_resubmission'] ?? null);
        $completed = sanitiseDate($row['date_completed']    ?? null);

        if (!$def_id) continue;

        $stmt->execute([
            $student_id, $def_id, $status,
            $date_set, $deadline, $resub, $completed,
            $user_id, $user_id,
        ]);
    }

    echo json_encode(['success' => true]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
