using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class FixKnowledgeLayerLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════════════════
            // FIX 1 — Convert PLANT_IRRIGATION_TEMPLATE.Water_amount
            //          from L/m²  →  m³/feddan  (1 feddan = 4200 m²)
            //          Guard: only rows still in the 1-10 L/m² range.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
UPDATE PLANT_IRRIGATION_TEMPLATE
SET    Water_amount = ROUND(Water_amount * 4.2, 2)
WHERE  Water_amount BETWEEN 1 AND 10;");

            // ═══════════════════════════════════════════════════════════════════
            // FIX 2 — Seed PLANT_STAGE + PLANT_IRRIGATION_TEMPLATE for Rice
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Rice', N'أرز'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 7, N'تنقع البذور 24 ساعة في ماء دافئ ثم تُبث في أسرة بذور رطبة. درجة حرارة 25-30 درجة مثالية. حافظ على رطوبة ثابتة لضمان تجانس الإنبات.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الشتل والتجذير')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الشتل والتجذير', 2, 21, N'تُشتل البادرات بعد 21-28 يوماً من الإنبات. تحتاج تغمير خفيف 2-3 سم لإرساء الجذور. ري منتظم يمنع جفاف التربة خلال فترة التكيف.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'التفريع')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'التفريع', 3, 35, N'تنمو الأفرع الجانبية بقوة. أهم مرحلة لتكوين عدد السنابل. التغمير المستمر 5-8 سم يدعم التفريع المكثف ويقمع الحشائش. ذروة الامتصاص الغذائي.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإسبال')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإسبال', 4, 21, N'السنبلة تتكون داخل الغمد. مرحلة حرجة جداً - الإجهاد المائي هنا يسبب عقماً كبيراً وخسارة في المحصول. حافظ على تغمير 5-10 سم بانتظام.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار والتلقيح')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإزهار والتلقيح', 5, 14, N'الأزهار تتفتح وتُلقح ذاتياً خلال ساعات الصباح. حافظ على تغمير خفيف. درجات حرارة عالية فوق 35 درجة تتلف حبوب اللقاح وتسبب تشيص السنابل.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج والحصاد')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النضج والحصاد', 6, 32, N'الحبوب تتصلب وتتحول إلى اللون الذهبي. أوقف الري 10-14 يوماً قبل الحصاد لتجفيف الحقل. الحصاد عند 20-25% رطوبة حبوب للحد من الكسر والخسارة.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 20.00, 2, N'يوم', N'20 م³/فدان كل يومين. حافظ على رطوبة أسرة البذور باستمرار. التغمير الخفيف يُسرّع الإنبات ويمنع جفاف سطح التربة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الشتل والتجذير');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الشتل والتجذير', 35.00, 3, N'يوم', N'35 م³/فدان كل 3 أيام. تغمير خفيف 2-3 سم يُثبّت البادرات ويُرسّخ الجذور. الجفاف في هذه الفترة يرفع نسبة النفوق.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'التفريع');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري التفريع', 40.00, 4, N'يوم', N'40 م³/فدان كل 4 أيام. التغمير المستمر 5-8 سم يُعظّم عدد الأفرع المنتجة ويقمع الحشائش التنافسية.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإسبال');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإسبال', 35.00, 3, N'يوم', N'35 م³/فدان كل 3 أيام. مرحلة لا تقبل التهاون في الري. الإجهاد المائي يسبب تشيصاً كاملاً وخسارة تصل 50% من المحصول.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار والتلقيح');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإزهار والتلقيح', 35.00, 3, N'يوم', N'35 م³/فدان كل 3 أيام. تغمير خفيف متواصل. تجنب ارتفاع الرطوبة الشديد ليلاً الذي يعزز الأمراض الفطرية.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج والحصاد');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النضج والحصاد', 20.00, 5, N'يوم', N'20 م³/فدان كل 5 أيام ثم الإيقاف الكامل 10-14 يوماً قبل الحصاد. الجفاف التدريجي يُسرّع النضج ويُيسّر حركة الحصادة.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // FIX 3 — Link IRRIGATION_STAGE.PSid + Duration_days
            //          Primary match: Stage_order  |  Fallback: Name_stage
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
UPDATE ir_s
SET    ir_s.PSid         = ps.PSid,
       ir_s.Duration_days = ISNULL(ir_s.Duration_days, ps.Duration_days)
FROM   IRRIGATION_STAGE ir_s
JOIN   CROP             c  ON  ir_s.Cid = c.Cid
JOIN   PLANT_STAGE      ps ON  ps.Pid   = c.Pid
                           AND (
                               (ir_s.Stage_order IS NOT NULL AND ps.Stage_order = ir_s.Stage_order)
                               OR
                               (ir_s.Stage_order IS NULL
                                AND LOWER(LTRIM(RTRIM(ps.Name_stage))) = LOWER(LTRIM(RTRIM(ir_s.Name_stage))))
                           )
WHERE  ir_s.PSid IS NULL;");

            // ═══════════════════════════════════════════════════════════════════
            // FIX 4 — Add missing Potato IRRIGATION_STAGE rows (stages 4-6)
            //          for every potato crop that currently has fewer than 6 stages.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
INSERT INTO IRRIGATION_STAGE (Name_stage, Stage_order, Description, Cid, PSid, Duration_days)
SELECT ps.Name_stage,
       ps.Stage_order,
       ps.Description,
       c.Cid,
       ps.PSid,
       ps.Duration_days
FROM   PLANT_STAGE ps
JOIN   PLANT       p  ON  ps.Pid  = p.Pid
JOIN   CROP        c  ON  c.Pid   = p.Pid
WHERE  p.Name IN (N'Potato', N'بطاطس')
  AND  ps.Stage_order > 3
  AND  NOT EXISTS (
           SELECT 1
           FROM   IRRIGATION_STAGE ir_s2
           WHERE  ir_s2.Cid         = c.Cid
             AND  ir_s2.Stage_order = ps.Stage_order
       );");

            // ═══════════════════════════════════════════════════════════════════
            // FIX 5 — Link IRRIGATION.PTid via the stage's resolved PSid
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
UPDATE ir
SET    ir.PTid = pt.PTid
FROM   IRRIGATION                ir
JOIN   IRRIGATION_STAGE          ir_s ON  ir.Sis  = ir_s.Sid
JOIN   PLANT_IRRIGATION_TEMPLATE pt   ON  pt.PSid = ir_s.PSid
WHERE  ir.PTid IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse FIX 5 — unlink IRRIGATION.PTid
            migrationBuilder.Sql(@"
UPDATE ir
SET    ir.PTid = NULL
FROM   IRRIGATION ir
JOIN   IRRIGATION_STAGE ir_s ON ir.Sis = ir_s.Sid
JOIN   CROP c ON ir_s.Cid = c.Cid
JOIN   PLANT p ON c.Pid = p.Pid
WHERE  p.Name IN (N'Potato',N'بطاطس',N'Rice',N'أرز',N'Wheat',N'قمح',N'Corn',N'ذرة',N'Maize');");

            // Reverse FIX 4 — delete added potato stages 4-6
            migrationBuilder.Sql(@"
DELETE ir_s
FROM   IRRIGATION_STAGE ir_s
JOIN   CROP  c ON ir_s.Cid = c.Cid
JOIN   PLANT p ON c.Pid    = p.Pid
WHERE  p.Name IN (N'Potato', N'بطاطس')
  AND  ir_s.Stage_order > 3;");

            // Reverse FIX 3 — unlink IRRIGATION_STAGE.PSid / Duration_days
            migrationBuilder.Sql(@"
UPDATE ir_s
SET    ir_s.PSid          = NULL,
       ir_s.Duration_days = NULL
FROM   IRRIGATION_STAGE ir_s
JOIN   CROP  c ON ir_s.Cid = c.Cid
JOIN   PLANT p ON c.Pid    = p.Pid
WHERE  p.Name IN (N'Potato',N'بطاطس',N'Rice',N'أرز',N'Wheat',N'قمح',N'Corn',N'ذرة',N'Maize');");

            // Reverse FIX 2 — delete rice PLANT_STAGE + templates
            migrationBuilder.Sql(@"
DELETE pt
FROM   PLANT_IRRIGATION_TEMPLATE pt
JOIN   PLANT_STAGE ps ON pt.PSid = ps.PSid
JOIN   PLANT       p  ON ps.Pid  = p.Pid
WHERE  p.Name IN (N'Rice', N'أرز');

DELETE ps
FROM   PLANT_STAGE ps
JOIN   PLANT       p ON ps.Pid = p.Pid
WHERE  p.Name IN (N'Rice', N'أرز');");

            // Reverse FIX 1 — convert Water_amount back from m³/feddan to L/m²
            migrationBuilder.Sql(@"
UPDATE PLANT_IRRIGATION_TEMPLATE
SET    Water_amount = ROUND(Water_amount / 4.2, 2)
WHERE  Water_amount BETWEEN 8 AND 50;");
        }
    }
}
