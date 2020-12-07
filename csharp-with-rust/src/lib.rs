#[no_mangle]
pub extern "C" fn int_add(int8: i8, int16: i16, int32: i32, int64: i64) -> i64 {
    int8 as i64 + int16 as i64 + int32 as i64 + int64
}

#[no_mangle]
pub extern "C" fn uint_add(uint8: u8, uint16: u16, uint32: u32, uint64: u64) -> u64 {
    uint8 as u64 + uint16 as u64 + uint32 as u64 + uint64
}

#[no_mangle]
pub extern "C" fn float_add(float32: f32, float64: f64) -> f64 {
    float32 as f64 + float64
}

#[no_mangle]
pub extern "C" fn string_count_ascii(string_ptr: *mut u8, len_elms: i32) -> i32 {
    string_count_utf8(string_ptr, len_elms)
}

#[no_mangle]
pub extern "C" fn string_count_ascii_with_null(string_ptr: *mut u8) -> i32 {
    string_count_utf8_with_null(string_ptr)
}

#[no_mangle]
pub extern "C" fn string_count_utf8(string_ptr: *mut u8, len_bytes: i32) -> i32 {
    let slice = unsafe {
        std::ptr::slice_from_raw_parts(string_ptr, len_bytes as usize)
            .as_ref()
            .unwrap()
    };

    let string = String::from_utf8_lossy(slice);
    string.chars().count() as i32
}

#[no_mangle]
pub extern "C" fn string_count_utf8_with_null(string_ptr: *mut u8) -> i32 {
    let mut null_i = 0;
    loop {
        unsafe {
            if (*string_ptr.add(null_i)) == 0x00 {
                break;
            }
        }
        null_i += 1;
    }

    string_count_utf8(string_ptr, null_i as i32)
}

#[no_mangle]
pub extern "C" fn string_count_utf16(string_ptr: *mut u16, len_bytes: i32) -> i32 {
    let slice = unsafe {
        std::ptr::slice_from_raw_parts(string_ptr, (len_bytes / 2) as usize)
            .as_ref()
            .unwrap()
    };

    let string = String::from_utf16(slice).unwrap();
    string.chars().count() as i32
}

#[no_mangle]
pub extern "C" fn string_count_utf16_with_null(string_ptr: *mut u16) -> i32 {
    let mut null_i = 0;
    loop {
        unsafe {
            if (*string_ptr.add(null_i)) == 0x0000 {
                break;
            }
        }
        null_i += 1;
    }

    string_count_utf16(string_ptr, (null_i * 2) as i32)
}

#[no_mangle]
pub extern "C" fn get_string_ascii(str_buf_ptr: *mut u8, buf_len_bytes: i32) -> i32 {
    let str = "Hello, C#!";
    let str_len_bytes = str.len() as i32;
    if buf_len_bytes < str_len_bytes {
        return -1;
    };
    unsafe { str_buf_ptr.copy_from_nonoverlapping(str.as_ptr(), str.len()) };
    str_len_bytes as i32
}

#[no_mangle]
pub extern "C" fn get_string_utf8(str_buf_ptr: *mut u8, buf_len_bytes: i32) -> i32 {
    let str = "Hello, C#! 日本語";
    let str_len_bytes = str.len() as i32;
    if buf_len_bytes < str_len_bytes {
        return -1;
    };
    unsafe { str_buf_ptr.copy_from_nonoverlapping(str.as_ptr(), str.len()) };
    str_len_bytes as i32
}

#[no_mangle]
pub extern "C" fn get_string_utf16(str_buf_ptr: *mut u16, buf_len_bytes: i32) -> i32 {
    let str = "Hello, C#! 日本語".encode_utf16().collect::<Vec<u16>>();
    let str_len_bytes = (str.len() * 2) as i32;
    if buf_len_bytes < str_len_bytes {
        return -1;
    };
    unsafe { str_buf_ptr.copy_from_nonoverlapping(str.as_ptr(), str.len()) };
    str_len_bytes as i32
}

#[no_mangle]
pub extern "C" fn array_function(array_ptr: *mut i32, len: i32) {
    let mut vector = unsafe { Vec::from_raw_parts(array_ptr, len as usize, len as usize) };
    for v in vector.iter_mut() {
        *v *= 2;
    }
}

#[no_mangle]
pub extern "C" fn get_values_via_out(int_ptr: *mut i32, enum1_ptr: *mut i32, enum2_ptr: *mut i32) {
    unsafe {
        *int_ptr = 123;
        *enum1_ptr = 1;
        enum2_ptr.write(2);
    };
}

#[no_mangle]
pub extern "C" fn get_bytes_with_index_as_value_buf_copy(bytes_ptr: *mut u8, len: i32) -> i32 {
    let bytes_to_write = (0..10).collect::<Vec<u8>>();
    if (len as usize) < bytes_to_write.len() {
        return -1;
    }

    unsafe {
        std::ptr::copy_nonoverlapping(bytes_to_write.as_ptr(), bytes_ptr, bytes_to_write.len())
    };
    bytes_to_write.len() as i32
}

#[no_mangle]
pub extern "C" fn get_bytes_with_index_as_value_ptr(bytes_ptr: *mut u8, len: i32) -> i32 {
    let len_to_write = 10;
    if (len as usize) < len_to_write {
        return -1;
    }

    for i in 0..len_to_write {
        unsafe { bytes_ptr.add(i).write(i as u8) };
    }
    len_to_write as i32
}

#[no_mangle]
pub extern "C" fn call_callback(callback: extern "C" fn()) {
    (callback)();
}

#[no_mangle]
pub extern "C" fn call_callback_with_utf8_string(callback: extern "C" fn(*const u8)) {
    let string_vec = "this is callback. これはコールバックです。";
    (callback)(string_vec.as_ptr());
}

#[repr(C)]
pub struct Strukt {
    num: i32,
}

#[no_mangle]
pub extern "C" fn double_struct_value(mut strukt: Strukt) -> Strukt {
    strukt.num *= 2;
    strukt
}

#[no_mangle]
pub extern "C" fn create_rust_obj() -> *mut Strukt {
    let b = Box::new(Strukt { num: 100 });
    Box::into_raw(b)
}

#[no_mangle]
pub extern "C" fn add_rust_obj_value(strukt_ptr: *mut Strukt, addition: i32) {
    let strukt = unsafe { &mut *strukt_ptr };
    strukt.num += addition;
}

#[no_mangle]
pub extern "C" fn get_rust_obj_value(strukt_ptr: *mut Strukt) -> i32 {
    let strukt = unsafe { &*strukt_ptr };
    strukt.num
}

#[no_mangle]
pub extern "C" fn dispose_rust_obj(strukt_ptr: *mut Strukt) {
    unsafe { Box::from_raw(strukt_ptr) };
}
