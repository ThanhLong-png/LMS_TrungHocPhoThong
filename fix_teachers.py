import re

content = open('Data/Taikhoan.cs', 'r', encoding='utf-8').read()

def replace_create_user(m):
    var_name = m.group(1) # e.g. gv1
    gv_num = int(var_name[2:])
    email = m.group(2)
    role = m.group(4)
    name = m.group(5)
    rest = m.group(6)
    
    ma_so = f"GV{gv_num:03d}"
    
    # determine gender based on name
    name_lower = name.lower()
    gioi_tinh = "Nam"
    if 'thị' in name_lower or 'nữ' in name_lower or 'như' in name_lower or 'hà' in name_lower or 'chi' in name_lower or 'yến' in name_lower or 'hoa' in name_lower or 'dung' in name_lower or 'phương' in name_lower or 'lan' in name_lower or ('anh' in name_lower and 'mai' in name_lower) or ('ngọc' in name_lower):
        gioi_tinh = "Nữ"
        
    if name in ["Nguyễn Thu Hà", "Lê Bảo Ngọc", "Hoàng Kim Chi", "Trần Mai Anh", "Đỗ Quỳnh Như", "Phạm Hải Yến", "Vũ Thị Phương", "Trần Thị Bình", "Phạm Thị Dung", "Bùi Thị Lan", "Lý Thị Hoa"]:
        gioi_tinh = "Nữ"
    
    # Password = ma_so
    return f'var {var_name} = await CreateUser(userManager, {email}, "{ma_so}", {role}, {name}, {rest}, userName: "{ma_so}", gioiTinh: "{gioi_tinh}");'

new_content = re.sub(r'var (gv\d+) = await CreateUser\(userManager, ("gv\d+@lms\.com"), ("Giaovien@1"), ("GiaoVien"), ("[^"]+"), (chuyenMon: [^\)]+)\);', replace_create_user, content)

with open('Data/Taikhoan.cs', 'w', encoding='utf-8') as f:
    f.write(new_content)
