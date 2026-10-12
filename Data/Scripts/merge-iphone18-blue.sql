-- One-time repair for the blue iPhone 18 Pro Max that was created as a separate product.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @targetId int = (SELECT Id FROM dbo.Products WHERE Slug = N'iphone-18-promax');
DECLARE @sourceId int = (SELECT Id FROM dbo.Products WHERE Slug = N'iphone-18-pro-max-xanh');

IF @sourceId IS NOT NULL
BEGIN
    IF @targetId IS NULL THROW 51010, 'Original iPhone 18 Pro Max is missing.', 1;
    IF EXISTS (SELECT 1 FROM dbo.OrderItems WHERE ProductId = @sourceId OR ProductVariantId IN
        (SELECT Id FROM dbo.ProductVariants WHERE ProductId = @sourceId))
        THROW 51011, 'Blue product has order history; manual migration required.', 1;
    IF EXISTS (SELECT 1 FROM dbo.ProductVariants WHERE ProductId = @targetId AND ColorName = N'Xanh' AND Storage = N'128GB')
        THROW 51012, 'Blue variant already exists on original product.', 1;

    UPDATE dbo.ProductVariants SET ProductId = @targetId, IsDefault = 0 WHERE ProductId = @sourceId;
    DELETE FROM dbo.Products WHERE Id = @sourceId;
END;

IF @targetId IS NOT NULL
BEGIN
    UPDATE dbo.ProductVariants
    SET ColorName = N'Đỏ Burgundy', ColorHex = N'#800020'
    WHERE ProductId = @targetId AND ColorName = N'Màu tiêu chuẩn' AND IsDefault = 1;
    UPDATE dbo.Products SET Color = N'#800020' WHERE Id = @targetId AND Color = N'#ddd6c8';
    UPDATE dbo.Products
    SET Description = REPLACE(Description,
        N'với 4 màu iPhone 18 Pro Max Đỏ Burgundy, Đen, Bạc và Băng Thanh',
        N'với các lựa chọn Đỏ Burgundy, Đen và Xanh tại cửa hàng')
    WHERE Id = @targetId AND Description LIKE N'%với 4 màu iPhone 18 Pro Max%';
END;

COMMIT TRANSACTION;
