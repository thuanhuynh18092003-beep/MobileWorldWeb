// Curated public sample data only. Never export accounts or orders from SQL Server here.
window.createDemoData = function () {
  const daysAgo = n => { const d = new Date(); d.setDate(d.getDate() - n); return d.toISOString(); };
  return {
    version: 1,
    colors: [{id:'c1',name:'Đỏ Burgundy',hex:'#800020'},{id:'c2',name:'Đen',hex:'#202020'},{id:'c3',name:'Xanh',hex:'#A4B9D8'},{id:'c4',name:'Xám',hex:'#A4A7A1'}],
    storages: [{id:'s1',name:'128GB'},{id:'s2',name:'256GB'},{id:'s3',name:'512GB'}],
    categories: [{id:'cat1',name:'iPhone',children:[{id:'sub1',name:'iPhone 16'},{id:'sub2',name:'iPhone 17'},{id:'sub3',name:'iPhone 18'}]},{id:'cat2',name:'Google',children:[{id:'sub4',name:'Pixel'}]}],
    products: [
      {id:'p1',name:'iPhone 18 Pro Max',brand:'Apple',categoryId:'sub3',description:'Một sắc thái mới cho trải nghiệm cao cấp. Chọn màu và dung lượng để xem ảnh, giá và tồn kho của đúng phiên bản.',screen:'6,9 inch OLED',chip:'Chip Pro thế hệ mới',camera:'Hệ thống 3 camera',battery:'Pin cho cả ngày',active:true,variants:[
        {id:'v11',colorId:'c1',storageId:'s1',price:80990000,stock:5,image:'assets/images/burgundy.webp',active:true},
        {id:'v12',colorId:'c2',storageId:'s1',price:80990000,stock:0,image:'assets/images/black.webp',active:true},
        {id:'v13',colorId:'c3',storageId:'s1',price:80990000,stock:0,image:'assets/images/blue.webp',active:true},
        {id:'v14',colorId:'c1',storageId:'s2',price:83990000,stock:8,image:'assets/images/burgundy.webp',active:true},
        {id:'v15',colorId:'c3',storageId:'s2',price:83990000,stock:6,image:'assets/images/blue.webp',active:true},
        {id:'v16',colorId:'c2',storageId:'s2',price:83990000,stock:4,image:'assets/images/black.webp',active:true}
      ]},
      {id:'p2',name:'Google Pixel 9 Pro',brand:'Google',categoryId:'sub4',description:'Thiết kế tối giản, nhiếp ảnh thông minh và trải nghiệm Android tinh tế. Một lựa chọn khác biệt cho công việc và sáng tạo mỗi ngày.',screen:'6,3 inch LTPO OLED',chip:'Tensor G4',camera:'Camera chính 50MP',battery:'4.700 mAh',active:true,variants:[
        {id:'v21',colorId:'c4',storageId:'s2',price:24990000,stock:9,image:'assets/images/pixel.webp',active:true},
        {id:'v22',colorId:'c4',storageId:'s3',price:28990000,stock:3,image:'assets/images/pixel.webp',active:true}
      ]}
    ],
    offers:[{id:'offer1',code:'GALAXY10',name:'Khám phá Galaxy',type:'Percent',value:10,min:5000000,max:1000000,active:true,limit:100,used:3}],
    cart: [],
    orders: [
      {id:'MW-DEMO-001',createdAt:daysAgo(6),completedAt:daysAgo(5),status:'Hoàn thành',name:'Khách mẫu 01',phone:'0900000000',address:'Địa chỉ mẫu',subtotal:80990000,discount:1000000,total:79990000,code:'GALAXY10',source:'seed',items:[{productId:'p1',variantId:'v11',name:'iPhone 18 Pro Max',label:'Đỏ Burgundy · 128GB',price:80990000,quantity:1}]},
      {id:'MW-DEMO-002',createdAt:daysAgo(4),completedAt:daysAgo(2),status:'Hoàn thành',name:'Khách mẫu 02',phone:'0900000000',address:'Địa chỉ mẫu',subtotal:24990000,discount:0,total:24990000,code:'',source:'seed',items:[{productId:'p2',variantId:'v21',name:'Google Pixel 9 Pro',label:'Xám · 256GB',price:24990000,quantity:1}]},
      {id:'MW-DEMO-003',createdAt:daysAgo(1),status:'Đang xử lý',name:'Khách mẫu 03',phone:'0900000000',address:'Địa chỉ mẫu',subtotal:83990000,discount:1000000,total:82990000,code:'GALAXY10',source:'seed',items:[{productId:'p1',variantId:'v14',name:'iPhone 18 Pro Max',label:'Đỏ Burgundy · 256GB',price:83990000,quantity:1}]}
    ]
  };
};
