"""Independently authored synthetic CPU/loader checks; no owned files needed."""
import struct
import unittest
from function_call import CallError, call
from le_image import LoaderError, decode_image


def synthetic_image(record=b''):
    module, header, pages = 0x26654, 0x290fc, 0x4c254
    data = bytearray(pages + 149 * 4096)
    def put(offset, value):
        struct.pack_into('<I', data, offset, value)
    struct.pack_into('<H', data, module, 0x5a4d)
    put(module + 0x3c, header - module)
    struct.pack_into('<H', data, header, 0x454c)
    for offset, value in [(0x14,149),(0x28,4096),(0x2c,4096),(0x40,0xc4),
                          (0x44,2),(0x48,0x200),(0x68,0x500),(0x6c,0x800),
                          (0x80,pages-module)]:
        put(header + offset,value)
    for index, values in enumerate([(0x7cb9e,0x10000,5,1,125),
                                     (0x23670,0x90000,3,126,24)]):
        for column,value in enumerate(values):
            put(header+0xc4+index*24+column*4,value)
    for page in range(149):
        offset=header+0x200+page*4
        data[offset:offset+3]=(page+1).to_bytes(3,'big')
    for page in range(1,150):
        put(header+0x500+page*4,len(record))
    data[header+0x800:header+0x800+len(record)]=record
    return data


class HarnessTests(unittest.TestCase):
    def objects(self, code):
        return [dict(base=0x10000,size=len(code),image=code,flags=5)]

    def test_arguments_and_reset(self):
        # Synthetic function adds two stack parameters and returns.
        objects=self.objects(bytes.fromhex('8b44240403442408c3'))
        self.assertEqual(call(objects,0x10000,(2,3))['result'],5)
        self.assertEqual(call(objects,0x10000,(9,7))['result'],16)

    def test_interrupt_rejected(self):
        with self.assertRaisesRegex(CallError,'interrupt'):
            call(self.objects(bytes.fromhex('cd21c3')),0x10000)

    def test_segment_reload_preserves_32_bit_stack_and_copy(self):
        # Authored function: save ES/ESI/EDI, reload ES from DS, copy ECX
        # dwords from two stack arguments, restore registers, return marker.
        code = bytes.fromhex(
            '0656571e07'  # push es; push esi; push edi; push ds; pop es
            '8b7424108b7c24148b4c2418f3a5'
            '5f5e07b878563412c3')
        objects = self.objects(code) + [
            dict(base=0x90000,size=4096,image=bytes(4096),flags=3)]
        source = bytes(range(24))
        for count in (0, 1, 6):
            with self.subTest(count=count):
                result = call(objects,0x10000,(0x90000,0x90100,count),
                              writes=((0x90000,source),
                                      (0x90100,b'?' * len(source))))
                self.assertEqual(result['result'],0x12345678)
                self.assertEqual(result['read'](0x90100,len(source)),
                                 source[:4*count] + b'?'*(len(source)-4*count))

    def test_descriptor_table_is_not_writable(self):
        # Authored absolute byte store into the synthetic descriptor page.
        with self.assertRaises(CallError):
            call(self.objects(bytes.fromhex('c6050000000400c3')),0x10000)

    def test_port_rejected(self):
        with self.assertRaisesRegex(CallError,'hardware/service'):
            call(self.objects(bytes.fromhex('e460c3')),0x10000)

    def test_budget_rejected(self):
        with self.assertRaisesRegex(CallError,'budget'):
            call(self.objects(bytes.fromhex('ebfe')),0x10000,instruction_limit=10)

    def test_mapping_padding_not_executable(self):
        with self.assertRaises(CallError):
            call(self.objects(bytes.fromhex('eb02')),0x10000)

    def test_fixup_and_zero_fill(self):
        objects,count=decode_image(synthetic_image(bytes.fromhex('07000800023412')))
        self.assertEqual(count,1)
        self.assertEqual(struct.unpack_from('<I',objects[0]['image'],8)[0],0x91234)
        self.assertEqual(objects[1]['image'][-16:],bytes(16))

    def test_wrapped_target_pointer(self):
        record=bytes.fromhex('0710080001f0ffffff')
        objects,_=decode_image(synthetic_image(record))
        self.assertEqual(struct.unpack_from('<I',objects[0]['image'],8)[0],0xfff0)

    def test_unsupported_fixup_rejected(self):
        with self.assertRaisesRegex(LoaderError,'Unsupported relocation'):
            decode_image(synthetic_image(bytes.fromhex('03000800023412')))

    def test_truncated_image_rejected(self):
        with self.assertRaises(LoaderError):
            decode_image(b'MZ')


if __name__=='__main__':
    unittest.main()
