using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class SeedKnowledgeLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // طماطم
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Tomato', N'طماطم'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 7, N'تمتص البذور رطوبة التربة وتنبت. حافظ على رطوبة منتظمة عند 80% من الطاقة الحقلية. درجة الحرارة المثالية للتربة 20-25 درجة مئوية. تجنب التشبع المائي لمنع تعفن البذور.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'البادرة', 2, 21, N'تظهر الأوراق الحقيقية الأولى. الري الخفيف المتكرر يشجع تعمق الجذور. تجنب الإفراط في الري لمنع مرض خنق البادرات. اسقِ عند القاعدة وحافظ على جفاف الأوراق.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النمو الخضري', 3, 35, N'توسع سريع للساق والأوراق. ري معتدل منتظم كل يومين. التغطية بالمهاد تقلل التبخر. يتعمق الجذر ويُنصح بالري حتى عمق 30 سم.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإزهار', 4, 14, N'مرحلة حرجة. الرطوبة المستمرة تمنع تساقط الأزهار. تجنب الإجهاد الجفافي والتشبع المائي. يُفضل الري بالتنقيط ولا تسقِ في ساعات الذروة الحارة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تطور الثمار', 5, 35, N'أعلى طلب مائي. الري المنتظم يمنع تعفن الطرف الزهري وتشقق الثمار. ري عميق كل 3 أيام. يجب أن تبقى التربة رطبة حتى عمق 30 سم.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النضج', 6, 14, N'قلل الري لتركيز السكريات وتجنب تشقق الثمار. اسقِ فقط عند ملاحظة ذبول خفيف. الماء الزائد يخفف السكريات ويسبب تشقق الثمار قرب الحصاد.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 3.00, 1, N'يوم', N'رش سطحي لطيف للحفاظ على رطوبة الـ5 سم العلوية بمعدل 3 لتر/م². استخدم رأس بخاخ ناعم وتجنب التدفق القوي الذي يزيح البذور.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري البادرة', 4.00, 2, N'يوم', N'ري لطيف معتدل كل يومين بمعدل 4 لتر/م². اسقِ بالقرب من منطقة الجذر وتجنب بلل الأوراق. اسقِ صباحاً للسماح لسطح التربة بالجفاف.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النمو الخضري', 5.00, 2, N'يوم', N'ري عميق معتدل كل يومين بمعدل 5 لتر/م² لتشجيع تعمق الجذور. استخدم الري بالتنقيط أو الأخاديد. المهاد يحافظ على الرطوبة بين الريّات.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإزهار', 6.00, 2, N'يوم', N'رطوبة منتظمة حرجة كل يومين بمعدل 6 لتر/م². لا تترك التربة تجف كلياً. الري بالتنقيط مثالي لأن تذبذب الرطوبة يرفع خطر تساقط الأزهار.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تطور الثمار', 7.00, 3, N'يوم', N'ري عميق كل 3 أيام بمعدل 7 لتر/م². الرطوبة غير المنتظمة تسبب تعفن الطرف الزهري والتشقق. حافظ على رطوبة التربة عند 70-80%.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النضج', 4.00, 4, N'يوم', N'قلل إلى 4 لتر/م² كل 4 أيام. الرطوبة الزائدة تسبب التشقق والطعم المائي. اسقِ فقط عند ظهور ذبول خفيف في الأوراق خلال الجزء الأبرد من اليوم.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // WHEAT / قمح
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Wheat', N'قمح'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 10, N'تمتص البذور الماء وتنبت خلال 10 أيام. التربة رطبة على عمق 5-10 سم. درجة الحرارة المثالية 12-22 درجة مئوية. تجنب التشبع المائي الذي يسبب تعفن البذور لاهوائياً.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'التفريخ')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'التفريخ', 2, 30, N'ظهور براعم ثانوية من التاج. الرطوبة الكافية تعزز التفريخ. جهاز الجذور التاجية نشط. تجنب التشبع لمنع تعفن الجذور والأمراض الفطرية.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'استطالة الساق')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'استطالة الساق', 3, 25, N'ارتفاع سريع وتكوين عقد وسلاميات. الري المعتدل يدعم النمو. الإفراط في النيتروجين والري يزيد خطر الرقود.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الطرد والإزهار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الطرد والإزهار', 4, 15, N'ظهور السنبلة وحدوث التلقيح. أحرج مراحل الري للقمح. الإجهاد الجفافي يسبب خسارة محصولية كبيرة. ضمان رطوبة كاملة حتى عمق 40 سم.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'امتلاء الحبوب')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'امتلاء الحبوب', 5, 30, N'تراكم النشا والبروتين في الحبوب. الري المعتدل يدعم وزن الحبوب. الإجهاد الحراري مع الجفاف يقلل المحتوى البروتيني ووزن الألف حبة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النضج', 6, 15, N'تجفيف وتصلب الحبوب. قلل وأوقف الري للسماح بالنضج المتجانس وتجنب الاخضرار مجدداً. احصد عند رطوبة الحبوب 12-14%.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 5.00, 3, N'يوم', N'ري ما قبل البذر لرفع رطوبة التربة إلى الطاقة الحقلية بمعدل 5 لتر/م² كل 3 أيام. الري بالرش أو الغمر. الرطوبة المنتظمة تضمن الإنبات المتجانس.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'التفريخ');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري التفريخ', 5.00, 5, N'يوم', N'ري معتدل كل 5 أيام بمعدل 5 لتر/م². يدعم نمو جذور التاج. تجنب التشبع الذي يعزز أمراض التعفن.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'استطالة الساق');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري استطالة الساق', 6.00, 7, N'يوم', N'ري أسبوعي بمعدل 6 لتر/م². التربة رطبة حتى 40 سم. قلل التكرار في الطقس البارد. الإفراط يعزز الرقود والأمراض الفطرية.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الطرد والإزهار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الطرد والإزهار', 7.00, 5, N'يوم', N'ري حرج كل 5 أيام بمعدل 7 لتر/م². رطوبة التربة يجب ألا تنخفض عن 60%. تجاوز هذا الري قد يقلل المحصول 20-40%.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'امتلاء الحبوب');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري امتلاء الحبوب', 6.00, 7, N'يوم', N'ري عميق أسبوعي بمعدل 6 لتر/م². الرياح الحارة الجافة تزيد الطلب. الرطوبة الكافية تقلل تجعد الحبوب وتحسن وزن الهكتولتر.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النضج', 2.00, 10, N'يوم', N'ري ضئيل كل 10 أيام فقط عند تهديد الذبول. أوقف الري 15 يوماً قبل الحصاد للسماح بجفاف الحقل بشكل منتظم.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // CORN / MAIZE / ذرة
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Corn', N'Maize', N'ذرة'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 7, N'امتصاص البذور للماء وظهور الجذير. درجة حرارة التربة 10 درجة كحد أدنى والمثالية 18-21 درجة مئوية. رطوبة منتظمة لإنبات متجانس. الصقيع المائي يتلف الجنين.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'البادرة', 2, 25, N'من V1 إلى V6، ظهور الأوراق ونمو الجذور العقدية. الري الخفيف يبني جهازاً جذرياً عميقاً. الذرة تتحمل جفافاً خفيفاً بين الريات. الرطوبة المنتظمة تحسن الإنتاجية.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النمو الخضري', 3, 30, N'من V7 إلى VT، ارتفاع سريع وزيادة مساحة الأوراق. يزداد الطلب المائي بحدة. إنشاء جهاز جذري عميق.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الطرد والتلقيح')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الطرد والتلقيح', 4, 14, N'أحرج المراحل. الطرد يطلق حبوب اللقاح. يوم واحد من الجفاف = خسارة 8% في المحصول. اسقِ دون انقطاع.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'امتلاء الحبوب')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'امتلاء الحبوب', 5, 35, N'من R3 إلى R6، تراكم النشا في الحبوب. استمرار الطلب المائي العالي. الإجهاد يقلل وزن الحبة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الاكتمال')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الاكتمال', 6, 14, N'تكون الطبقة السوداء في قاعدة الحبة R6. تنخفض رطوبة الحبوب من 35% إلى 25%. قلل وأوقف الري.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 4.00, 2, N'يوم', N'4 لتر/م² كل يومين. التربة رطبة على عمق 5 سم. تجنب التقشر السطحي. الرش الخفيف أفضل من الغمر.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري البادرة', 5.00, 3, N'يوم', N'5 لتر/م² كل 3 أيام. ري حتى عمق 30 سم لتشجيع تعمق الجذور. فترات جفاف خفيفة مقبولة بين الريات.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النمو الخضري', 6.00, 3, N'يوم', N'6 لتر/م² كل 3 أيام. ري حتى 45 سم. يتزايد معدل استخدام المياه بسرعة. راقب رطوبة التربة مع تطور المظلة الخضراء.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الطرد والتلقيح');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الطرد والتلقيح', 8.00, 2, N'يوم', N'حرج: 8 لتر/م² كل يومين. لا تسمح أبداً بالذبول. كل يوم جفاف = خسارة 8% في المحصول. هذا الري يعطي أعلى عائد.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'امتلاء الحبوب');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري امتلاء الحبوب', 7.00, 3, N'يوم', N'7 لتر/م² كل 3 أيام. الرطوبة المستدامة تضمن اكتمال الحبوب. الإجهاد R4-R5 يقلل وزن الحبة 15-25%.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الاكتمال');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الاكتمال', 3.00, 7, N'يوم', N'قلل إلى 3 لتر/م² أسبوعياً. أوقف الري 10 أيام قبل الحصاد لتجفيف الحقل طبيعياً.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // POTATO / بطاطس
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Potato', N'بطاطس'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 14, N'كسر الدرنة وظهور البراعم. درجة حرارة التربة المثالية 8-12 درجة مئوية. حافظ على رطوبة معتدلة. تجنب التشبع الذي يسبب تعفن قطع الدرنة البذرية.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'التطور الخضري')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'التطور الخضري', 2, 30, N'توسع السيقان والأوراق وتطور الجهاز الجذري والمدادات. الري المنتظم يعزز شبكة المدادات. يُنصح بالتخضيم لتشجيع نمو مدادات إضافية.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'بدء تكوين الدرنات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'بدء تكوين الدرنات', 3, 20, N'بدء انتفاخ المدادات لتكوين الدرنات. مرحلة حرجة جداً. الرطوبة الثابتة ضرورية لعدد الدرنات. درجة حرارة التربة يجب ألا تتجاوز 25 درجة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تضخم الدرنات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تضخم الدرنات', 4, 40, N'تراكم سريع للمادة الجافة. أعلى تركيز للطلب المائي. الري غير المنتظم يسبب تجاويف داخلية وتشققات وتشوهات. حافظ على رطوبة 70-80%.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الاكتمال')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الاكتمال', 5, 20, N'اصفرار الأوراق وجفافها. تصلب قشرة الدرنة. قلل الري تدريجياً. الرطوبة الزائدة تؤخر تصلب القشرة وتزيد مخاطر تعفن اللفحاء المتأخر .');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'التحضير للحصاد')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'التحضير للحصاد', 6, 7, N'أوقف الري 7-10 أيام قبل الحصاد. يصلب الدرنات ويجفف التربة لتسهيل الحصاد الآلي. يقلل التلف الميكانيكي أثناء الخدمة.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 4.00, 3, N'يوم', N'4 لتر/م² كل 3 أيام. رطبة بشكل منتظم دون تشبع. تعفن قطعة البذرة شائع في التربة الباردة الرطبة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'التطور الخضري');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري التطور الخضري', 5.00, 4, N'يوم', N'5 لتر/م² كل 4 أيام. ري حتى 30 سم. تربة التخضيم رطبة بما يكفي للتضغيط بإحكام حول الجذوع.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'بدء تكوين الدرنات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري بدء الدرنات', 7.00, 3, N'يوم', N'حرج: 7 لتر/م² كل 3 أيام. رطوبة التربة فوق 65%. حدث جفاف واحد يقلل عدد الدرنات 30%.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تضخم الدرنات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تضخم الدرنات', 8.00, 3, N'يوم', N'ذروة الطلب: 8 لتر/م² كل 3 أيام. حافظ على 70-80% طاقة حقلية. دورات الرطب-جاف تسبب عيوباً داخلية.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الاكتمال');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الاكتمال', 4.00, 5, N'يوم', N'تدريجياً إلى 4 لتر/م² كل 5 أيام مع جفاف الأوراق. الإيقاف المبكر يؤخر تصلب القشرة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'التحضير للحصاد');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري ما قبل الحصاد', 2.00, 7, N'يوم', N'2 لتر/م² فقط عند الحرارة الشديدة. الهدف تجفيف التربة وتصليب قشرة الدرنة لتقليل الكدمات.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // CUCUMBER / خيار
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Cucumber', N'خيار'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 5, N'إنبات سريع عند 25-30 درجة خلال 3-5 أيام. ظروف دافئة رطبة ضرورية. تجنب التربة الباردة دون 15 درجة التي تبطئ الإنبات.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'البادرة', 2, 14, N'توسع الفلقتين وظهور الورقة الحقيقية الأولى. الري المعتدل يبني عمق الجذر. عرضة لخنق البادرات - تجنب الإفراط في الري.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النمو الخضري', 3, 21, N'استطالة سريعة للكروم. يبدأ الطلب المائي العالي. الري المنتظم يمنع الطعم المر الناجم عن إجهاد الماء.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإزهار', 4, 14, N'ظهور أزهار ذكر وأنثى. الرطوبة الثابتة تمنع تساقط الأزهار. تجنب درجات حرارة فوق 35 درجة خلال الإزهار.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تطور الثمار', 5, 30, N'الثمار 95% ماء - طلب ري عالي. الحصاد كل 2-3 أيام يشجع الإنتاج. الإجهاد يسبب ثماراً مرة أو مشوهة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الحصاد')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الحصاد', 6, 14, N'مرحلة الحصاد المستمر. حافظ على الري طوال الفترة. الحصاد المنتظم والرطوبة الكافية يبقيان النبات منتجاً.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 3.00, 1, N'يوم', N'ري يومي لطيف بمعدل 3 لتر/م². حافظ على رطوبة الـٛ5 سم العليا. الماء الدافئ مفضل.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري البادرة', 4.00, 2, N'يوم', N'4 لتر/م² كل يومين. اسقِ على مستوى التربة. تهوية جيدة تمنع الأمراض الفطرية في أسرة البادرات.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النمو الخضري', 5.00, 2, N'يوم', N'5 لتر/م² كل يومين. جذور ضحلة - حافظ على رطوبة الـٛ30 سم العليا. المهاد يحافظ على الرطوبة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإزهار', 6.00, 2, N'يوم', N'6 لتر/م² كل يومين. رطوبة ثابتة تمنع إجهاض الأزهار. التنقيط يقلل أمراض الأوراق الحساسة للرطوبة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تطور الثمار', 7.00, 2, N'يوم', N'7 لتر/م² كل يومين. ذروة الطلب المائي. الإجهاد يسبب ثماراً منحنية مرة أو صغيرة. الري يجب أن يكون منتظماً جداً طوال هذه الفترة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الحصاد');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الحصاد', 5.00, 3, N'يوم', N'5 لتر/م² كل 3 أيام. حافظ على جودة الإنتاج بالرطوبة المستمرة. تقليل الري يؤدي إلى ثمار مرة وتراجع مبكر للنبات.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // WATERMELON / بطيخ
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Watermelon', N'بطيخ'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 7, N'إنبات عند 25-35 درجة. ظروف دافئة رطبة ضرورية. التربة الباردة دون 18 درجة تسبب تعفن البذور وعدم تجانس الإنبات.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'البادرة', 2, 14, N'الفلقتان والأوراق الحقيقية الأولى. الجذور تنشأ. ري معتدل. البطيخ يفضل ظروفاً أجف قليلاً في مرحلة البادرة لتشجيع تعمق الجذر الرئيسي.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'نمو الكروم')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'نمو الكروم', 3, 28, N'استطالة سريعة للكروم وتكوين فروع جانبية. الري المنتظم يدعم النمو القوي. تباعد الكروم يسمح بتهوية جيدة وتقليل ضغط البياض الدقيقي.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار والتلقيح')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإزهار والتلقيح', 4, 14, N'الأزهار الذكرية تسبق الأنثوية 7-10 أيام. نشاط النحل ضروري. الرطوبة الثابتة تمنع تساقط الأزهار. تجنب الري المسائي الذي يرفع الرطوبة ويعزز الأمراض الفطرية.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تطور الثمار', 5, 40, N'تكبر الثمرة من حجم الرخامة إلى وزنها الكامل. طلب مائي عالٍ. الري المتناسق يمنع التشوهات والفراغات الداخلية. فترة ري حرجة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النضج', 6, 14, N'تراكم السكريات وتطور اللون. قلل الري لتركيز السكريات وتحسين الطعم. الرطوبة الزائدة تخفف محتوى الباريكس وتعطي ثماراً مائية فاترة.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 4.00, 2, N'يوم', N'4 لتر/م² كل يومين. التربة رطبة جيدة التصريف. الرمل الطيني مفضل. تجنب التقشر السطحي برأس بخاخ ناعم.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري البادرة', 5.00, 3, N'يوم', N'5 لتر/م² كل 3 أيام. الري المعتدل يشجع تعمق الجذر الرئيسي. حافظ على جفاف الأوراق.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'نمو الكروم');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري نمو الكروم', 6.00, 3, N'يوم', N'6 لتر/م² كل 3 أيام. الكروم تشرب بكثرة مع التوسع. التنقيط على طول الكروم يبقي منطقة الثمار جافة ويقلل الأمراض. المهاد يحافظ على الرطوبة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار والتلقيح');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري التلقيح', 7.00, 3, N'يوم', N'7 لتر/م² كل 3 أيام. اسقِ صباحاً لتجف التربة بعد الظهر ولا تثبط نشاط النحل. الرطوبة الثابتة تمنع إجهاض الأزهار.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تطور الثمار', 8.00, 4, N'يوم', N'ذروة الطلب: 8 لتر/م² كل 4 أيام. الجذور العميقة تصل للرطوبة التحت-سطحية. التنقيط لمنطقة الجذر الأكثر كفاءة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النضج', 5.00, 5, N'يوم', N'قلل إلى 5 لتر/م² كل 5 أيام. أوقف 7 أيام قبل الحصاد لأقصى محتوى سكري. جفاف الخيط الأقرب للثمرة يدل على النضج.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // ONION / بصل
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Onion', N'بصل'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 10, N'إنبات بطيء 7-10 أيام عند 15-20 درجة. بذور البصل ذات طاقة إنباتية منخفضة. حافظ على رطوبة ثابتة. لا تترك سطح التربة يجف خلال هذه الفترة الحساسة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'البادرة', 2, 28, N'بادرات خيطية هشة تظهر. ضعيفة وذات جذور ضحيلة. حافظ على رطوبة الـ10 سم العليا. الري اللطيف المنخفض الضغط يمنع رقود البادرات.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الأوراق')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تطور الأوراق', 3, 35, N'يزيد عدد الأوراق إلى 13-16 ورقة. كل ورقة تقابل حرشفة في البصلة النهائية. الرطوبة الكافية تدعم التطور الكامل للأوراق. ذروة امتصاص النيتروجين.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'بدء تكوين البصلة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'بدء تكوين البصلة', 4, 20, N'يُحفَّز بطول النهار. قواعد الأوراق تنتفخ لتكوين الحراشيف. الرطوبة الثابتة تدعم تشكيل البصلة المنتظم. أوقف تسميد النيتروجين الآن لتجنب تأخر التبصيل.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور البصلة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تطور البصلة', 5, 30, N'تراكم السكريات والمواد الجافة في الحراشيف. طلب مائي عالٍ. الري غير المنتظم يسبب الانشطار الداخلي والبصلات المضاعفة. حافظ على رطوبة 50-60%.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الاكتمال')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الاكتمال', 6, 14, N'سقوط الأوراق يدل على الجاهزية. أوقف الري 14 يوماً قبل الحصاد للسماح بجفاف القشرة الخارجية وتكوين الغلاف الورقي. التجفيف يحسن عمر التخزين.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 3.00, 2, N'يوم', N'3 لتر/م² كل يومين بالرش الناعم. لا تترك السطح يجف خلال فترة الإنبات البطيء. رأس رذاذ ناعم يمنع إزاحة البذور.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري البادرة', 4.00, 3, N'يوم', N'4 لتر/م² كل 3 أيام. تطبيق لطيف جداً. بادرات البصل تُزاح بسهولة. التنقيط تحت السطح مثالي لتجنب إزعاج البادرات الهشة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الأوراق');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تطور الأوراق', 5.00, 4, N'يوم', N'5 لتر/م² كل 4 أيام. كل ورقة تصبح حرشفة تخزين. العدد الكامل = أكبر بصلة. الرطوبة المنتظمة حرجة لأقصى تكوين أوراق.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'بدء تكوين البصلة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري بدء البصلة', 6.00, 4, N'يوم', N'6 لتر/م² كل 4 أيام. أوقف التسميد النيتروجيني الآن. الرطوبة الثابتة تدعم تكوين الحراشيف المنتظم.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور البصلة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تطور البصلة', 5.00, 5, N'يوم', N'5 لتر/م² كل 5 أيام. الري المنتظم يقلل الانشطار والبصلات المضاعفة. تجنب الإفراط الذي يسبب تعفن العنق.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الاكتمال');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الاكتمال', 2.00, 7, N'يوم', N'2 لتر/م² أسبوعياً ثم الإيقاف 14 يوماً قبل الحصاد. البصل الجاف عند الحصاد له عمر تخزيني 3-6 أشهر مقابل أسابيع للبصل الرطب.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // PEPPER / فلفل
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Pepper', N'فلفل'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 10, N'إنبات بطيء 8-14 يوماً. درجة الحرارة المثالية للتربة 25-30 درجة. ظروف دافئة رطبة ثابتة ضرورية. حساس جداً للتربة الباردة دون 18 درجة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'البادرة', 2, 28, N'نمو بطيء. الأوراق الحقيقية تظهر تدريجياً. رطوبة معتدلة ثابتة للبادرات الصحية. تجنب الري الليلي لأن بلل الأوراق ليلاً يعزز خنق البادرات.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النمو الخضري', 3, 30, N'تفريع وتطور المظلة. الجهاز الجذري يتوسع. الري العميق المنتظم يعزز الجذر الواسع. مظلة الفلفل تظلل التربة جيداً وتقلل التبخر.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإزهار', 4, 21, N'أزهار ذاتية التلقيح. الرطوبة الثابتة ضرورية - الجفاف والتشبع يسببان تساقط الأزهار. امتصاص الكالسيوم حرج لمنع تعفن الطرف الزهري.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تطور الثمار', 5, 35, N'الثمار تنمو 5-10 أسابيع. الفلفل يستمر في تعقيد ثمار جديدة مع الحصاد. الري المنتظم يمنع الإجهاد المائي الذي يسبب تغير اللون المبكر وصغر الحجم.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الحصاد')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الحصاد', 6, 14, N'احصد أخضر أو أحمر حسب متطلبات السوق. الحصاد المستمر يشجع تعقيد ثمار إضافية. حافظ على الري طوال موسم الحصاد لاستمرار الإنتاج.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 3.00, 1, N'يوم', N'3 لتر/م² يومياً. التربة دافئة ورطبة بشكل منتظم. الغطاء البلاستيكي يحافظ على درجة حرارة التربة. استخدم ماء بدرجة الحرارة المحيطة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري البادرة', 4.00, 2, N'يوم', N'4 لتر/م² كل يومين. اسقِ عند قاعدة النبات. تجنب بلل الساق. اسقِ صباحاً لتجف الأوراق قبل المساء.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النمو الخضري', 5.00, 3, N'يوم', N'5 لتر/م² كل 3 أيام. ري عميق حتى 40 سم. تجيب جذور الفلفل للرطوبة المعتدلة الثابتة. تجنب الجفاف والتشبع في جميع الأوقات.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإزهار', 6.00, 3, N'يوم', N'6 لتر/م² كل 3 أيام. الرطوبة الثابتة تمنع تساقط الأزهار. رش الكالسيوم بالتزامن مع الري يمنع تعفن الطرف الزهري.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تطور الثمار', 7.00, 3, N'يوم', N'7 لتر/م² كل 3 أيام. طلب تبخر-نتح عالٍ خلال الإثمار. التنقيط عند قاعدة النبات يقلل رطوبة الأوراق والأمراض الورقية.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الحصاد');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الحصاد', 5.00, 4, N'يوم', N'5 لتر/م² كل 4 أيام. حافظ على الرطوبة طوال موسم الحصاد. الحصاد كل 5-7 أيام يُبقي النبات منتجاً لأشهر.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // EGGPLANT / AUBERGINE / باذنجان
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Eggplant', N'Aubergine', N'باذنجان'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 10, N'يحتاج تربة دافئة 25-30 درجة للإنبات خلال 8-10 أيام. حافظ على أسرة البذور رطبة بانتظام. حساس جداً للتربة الباردة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'البادرة', 2, 28, N'نمو مبكر بطيء. يحتاج درجات حرارة 22-28 درجة. ري معتدل منتظم. اشتل عند ارتفاع 15-20 سم بـ4-5 أوراق حقيقية. قسِّ قبل الزراعة الحقلية.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النمو الخضري', 3, 35, N'نمو شجيري قوي وتطور جذر وتدي عميق. ري عميق منتظم. أكثر تحملاً للجفاف من الطماطم بعد الإرساء لكن الإجهاد المستمر يقلل المحصول كثيراً.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإزهار', 4, 21, N'أزهار بنفسجية كبيرة ذاتية الإخصاب. الرطوبة الثابتة حرجة لعقد الثمار. نقص الكالسيوم والبورون مع الري غير المنتظم يسببان تساقط الأزهار.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تطور الثمار', 5, 40, N'ثمار كثيفة غنية بالماء تحتاج ريًا منتظماً 40-60 يوماً. اقطف قبل النضج التام بينما القشرة لامعة. الرطوبة غير المنتظمة تعطي ثماراً مرة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الحصاد')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الحصاد', 6, 14, N'الحصاد المنتظم يحفز تعقيد ثمار جديدة. النباتات تنتج 4-6 أشهر في المناخات الدافئة. حافظ على الري طوال موسم الحصاد.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 3.00, 1, N'يوم', N'3 لتر/م² يومياً برأس رذاذ ناعم. الغطاء البلاستيكي يحسن الإنبات بالحفاظ على دفء التربة ليلاً.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري البادرة', 4.00, 2, N'يوم', N'4 لتر/م² كل يومين. اسقِ على مستوى التربة. قسِّ النبات تدريجياً 7-10 أيام قبل الزراعة الحقلية.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النمو الخضري');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النمو الخضري', 5.00, 3, N'يوم', N'5 لتر/م² كل 3 أيام. ري عميق حتى 45 سم. الأخاديد أو التنقيط أفضل من الرش الرأسي الذي يبلل الأوراق.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإزهار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإزهار', 6.00, 3, N'يوم', N'6 لتر/م² كل 3 أيام. الرطوبة الثابتة حرجة لعقد الثمار. نقص الكالسيوم مع الري غير المنتظم يسبب تساقط الأزهار - رش كالسيوم وقائي.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الثمار');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تطور الثمار', 7.00, 3, N'يوم', N'7 لتر/م² كل 3 أيام. الثمار ~90% ماء. الرطوبة الثابتة تمنع الطعم المر. الإجهاد يعطي ثماراً صغيرة مرة بجودة سوقية رديئة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الحصاد');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الحصاد', 5.00, 4, N'يوم', N'5 لتر/م² كل 4 أيام طوال موسم الحصاد. لا تقلل الري بين القطفات. النباتات المجهدة تنتج ثماراً صغيرة متشددة بقيمة سوقية ضعيفة.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // LETTUCE / خس
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
DECLARE @Pid INT = (SELECT TOP 1 Pid FROM PLANT WHERE Name IN (N'Lettuce', N'خس'));
IF @Pid IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'الإنبات', 1, 5, N'إنبات سريع 2-5 أيام عند 15-20 درجة. حساس للحرارة - لا ينبت فوق 30 درجة. زراعة على عمق 3-5 ملم. الرذاذ الخفيف يحافظ على الرطوبة دون إزاحة البذور.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'البادرة', 2, 14, N'الفلقتان والأوراق الحقيقية الأولى. جهاز جذري ضحيل جداً. ري خفيف متكرر ضروري. الجذور تتلف بسهولة من الجفاف. حافظ على رطوبة الـ5 سم العليا دائماً.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الأوراق')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تطور الأوراق', 3, 21, N'توسع سريع للأوراق الخارجية. محتوى الماء العالي يتطلب ريًا ثابتاً. حرق الطرف (نقص الكالسيوم في الأوراق الداخلية) يحدث مع الري غير المنتظم.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تكوين الرأس')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'تكوين الرأس', 4, 21, N'تنطوي الأوراق الداخلية وتتكثف. وزن الرأس يعتمد على درجات 15-18 درجة ورطوبة كافية. الجفاف يسبب رؤوساً مفتوحة فضفاضة.');
    IF NOT EXISTS (SELECT 1 FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج والحصاد')
        INSERT INTO PLANT_STAGE (Pid, Name_stage, Stage_order, Duration_days, Description)
        VALUES (@Pid, N'النضج والحصاد', 5, 7, N'الرؤوس صلبة وبالحجم الكامل. احصد فوراً - التأخير يسبب الطرد (الإزهار) خاصة في الطقس الدافئ. الخس المطرود يصبح مراً وغير قابل للتسويق.');

    DECLARE @PS INT;
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'الإنبات');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري الإنبات', 2.00, 1, N'يوم', N'رذاذ خفيف يومي بمعدل 2 لتر/م². لا يجب أن يجف السطح بين التطبيقات. رأس بخاخ ناعم جداً ضروري لزراعة الخس الضحيلة.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'البادرة');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري البادرة', 3.00, 2, N'يوم', N'3 لتر/م² كل يومين. ضحيل الجذور - حافظ على رطوبة الـ15 سم العليا. الرش مناسب مع تصريف كافٍ لمنع خنق البادرات.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تطور الأوراق');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تطور الأوراق', 4.00, 3, N'يوم', N'4 لتر/م² كل 3 أيام. رطوبة ثابتة لتوسع الأوراق السريع. الري الثابت يمنع حرق الطرف. أضف نترات الكالسيوم عبر ماء الري عند ظهور الأعراض.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'تكوين الرأس');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري تكوين الرأس', 4.00, 3, N'يوم', N'4 لتر/م² كل 3 أيام. الرطوبة الثابتة أثناء امتلاء الرأس تزيد كثافته ووزنه. الخس الرأسي حساس جداً للجفاف.');
    SET @PS = (SELECT TOP 1 PSid FROM PLANT_STAGE WHERE Pid = @Pid AND Name_stage = N'النضج والحصاد');
    IF @PS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PLANT_IRRIGATION_TEMPLATE WHERE PSid = @PS)
        INSERT INTO PLANT_IRRIGATION_TEMPLATE (PSid, Pid, Irrigation_name, Water_amount, Frequency_value, Frequency_unit, Description)
        VALUES (@PS, @Pid, N'ري النضج والحصاد', 3.00, 4, N'يوم', N'3 لتر/م² كل 4 أيام. حافظ على الرطوبة حتى يوم الحصاد. أوقف الري 12 ساعة قبل الحصاد لرؤوس مقطوعة أنظف وأطول صلاحية.');
END");

            // ═══════════════════════════════════════════════════════════════════
            // LINK existing IRRIGATION_STAGE -> PLANT_STAGE
            //      and  IRRIGATION          -> PLANT_IRRIGATION_TEMPLATE
            // Match is done by plant (via CROP.Pid) and exact stage name (case-insensitive).
            // Only updates rows where the FK is still NULL (safe to re-run).
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
UPDATE ir_s
SET    ir_s.PSid         = ps.PSid,
       ir_s.Duration_days = ISNULL(ir_s.Duration_days, ps.Duration_days)
FROM   IRRIGATION_STAGE ir_s
JOIN   CROP             c  ON  ir_s.Cid = c.Cid
JOIN   PLANT            p  ON  c.Pid    = p.Pid
JOIN   PLANT_STAGE      ps ON  ps.Pid   = p.Pid
                           AND LOWER(LTRIM(RTRIM(ps.Name_stage))) = LOWER(LTRIM(RTRIM(ir_s.Name_stage)))
WHERE  ir_s.PSid IS NULL;

UPDATE ir
SET    ir.PTid = pt.PTid
FROM   IRRIGATION                ir
JOIN   IRRIGATION_STAGE          ir_s ON  ir.Sis   = ir_s.Sid
JOIN   PLANT_IRRIGATION_TEMPLATE pt   ON  pt.PSid  = ir_s.PSid
                                      AND LOWER(LTRIM(RTRIM(pt.Irrigation_name))) = LOWER(LTRIM(RTRIM(ir.Irrigation_name)))
WHERE  ir.PTid IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Unlink IRRIGATION rows that point to templates we seeded
            migrationBuilder.Sql(@"
UPDATE ir
SET    ir.PTid = NULL
FROM   IRRIGATION ir
JOIN   PLANT_IRRIGATION_TEMPLATE pt ON ir.PTid = pt.PTid
JOIN   PLANT_STAGE               ps ON pt.PSid  = ps.PSid
JOIN   PLANT                     p  ON ps.Pid   = p.Pid
WHERE  p.Name IN (N'Tomato',N'طماطم',N'Wheat',N'قمح',N'Corn',N'Maize',N'ذرة',
                  N'Potato',N'بطاطس',N'Cucumber',N'خيار',N'Watermelon',N'بطيخ',
                  N'Onion',N'بصل',N'Pepper',N'فلفل',N'Eggplant',N'Aubergine',N'باذنجان',
                  N'Lettuce',N'خس');");

            // 2. Unlink IRRIGATION_STAGE rows that point to stages we seeded
            migrationBuilder.Sql(@"
UPDATE ir_s
SET    ir_s.PSid          = NULL,
       ir_s.Duration_days = NULL
FROM   IRRIGATION_STAGE ir_s
JOIN   PLANT_STAGE      ps ON ir_s.PSid = ps.PSid
JOIN   PLANT            p  ON ps.Pid    = p.Pid
WHERE  p.Name IN (N'Tomato',N'طماطم',N'Wheat',N'قمح',N'Corn',N'Maize',N'ذرة',
                  N'Potato',N'بطاطس',N'Cucumber',N'خيار',N'Watermelon',N'بطيخ',
                  N'Onion',N'بصل',N'Pepper',N'فلفل',N'Eggplant',N'Aubergine',N'باذنجان',
                  N'Lettuce',N'خس');");

            // 3. Delete seeded PLANT_IRRIGATION_TEMPLATE rows
            migrationBuilder.Sql(@"
DELETE pt
FROM   PLANT_IRRIGATION_TEMPLATE pt
JOIN   PLANT_STAGE               ps ON pt.PSid = ps.PSid
JOIN   PLANT                     p  ON ps.Pid  = p.Pid
WHERE  p.Name IN (N'Tomato',N'طماطم',N'Wheat',N'قمح',N'Corn',N'Maize',N'ذرة',
                  N'Potato',N'بطاطس',N'Cucumber',N'خيار',N'Watermelon',N'بطيخ',
                  N'Onion',N'بصل',N'Pepper',N'فلفل',N'Eggplant',N'Aubergine',N'باذنجان',
                  N'Lettuce',N'خس');");

            // 4. Delete seeded PLANT_STAGE rows
            migrationBuilder.Sql(@"
DELETE ps
FROM   PLANT_STAGE ps
JOIN   PLANT       p  ON ps.Pid = p.Pid
WHERE  p.Name IN (N'Tomato',N'طماطم',N'Wheat',N'قمح',N'Corn',N'Maize',N'ذرة',
                  N'Potato',N'بطاطس',N'Cucumber',N'خيار',N'Watermelon',N'بطيخ',
                  N'Onion',N'بصل',N'Pepper',N'فلفل',N'Eggplant',N'Aubergine',N'باذنجان',
                  N'Lettuce',N'خس');");
        }
    }
}
