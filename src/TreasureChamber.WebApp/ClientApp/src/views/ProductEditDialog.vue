<template>
  <Dialog :visible="visible" @update:visible="v => emit('update:visible', v)" :header="isEdit ? '编辑产品' : '新建产品'" :style="{ width: '720px' }" modal maximizable>
    <div v-if="formError" class="mb-3">
      <Message severity="error" :closable="false">{{ formError }}</Message>
    </div>

    <div class="grid">
      <div class="col-12 md:col-6">
        <label class="font-bold block mb-1">型号 <span class="text-danger">*</span></label>
        <InputText v-model="form.model" class="w-full" />
      </div>
      <div class="col-12 md:col-6">
        <label class="font-bold block mb-1">名称</label>
        <InputText v-model="form.name" class="w-full" placeholder="留空默认使用型号" />
      </div>
      <div class="col-12 md:col-4">
        <label class="font-bold block mb-1">系列</label>
        <Dropdown v-model="form.seriesId" :options="series" option-label="name" option-value="id"
                  placeholder="（无）" class="w-full" show-clear />
      </div>
      <div class="col-12 md:col-4">
        <label class="font-bold block mb-1">分类</label>
        <Dropdown v-model="form.categoryId" :options="categories" option-label="label" option-value="id"
                  placeholder="（无）" class="w-full" show-clear />
      </div>
      <div class="col-12 md:col-4">
        <label class="font-bold block mb-1">状态</label>
        <SelectButton v-model="form.isActive" :options="[{label:'在售',value:true},{label:'停用',value:false}]"
                      option-label="label" option-value="value" />
      </div>
      <div class="col-12">
        <label class="font-bold block mb-1">产品介绍</label>
        <Textarea v-model="form.description" rows="4" class="w-full" placeholder="产品卖点、适用场景等介绍文字" />
      </div>
    </div>

    <h6 class="font-bold mt-4 mb-2">规格参数</h6>
    <div v-for="(spec, i) in specs" :key="i" class="d-flex gap-2 mb-2">
      <InputText v-model="spec.name" placeholder="参数名，如 功率" class="w-3" />
      <InputText v-model="spec.value" placeholder="参数值，如 36W" class="flex-1" />
      <Button icon="pi pi-trash" severity="danger" text rounded @click="specs.splice(i, 1)" />
    </div>
    <Button label="添加一行" icon="pi pi-plus" size="small" text @click="specs.push({ name: '', value: '' })" />

    <!-- 已有产品：图片管理 -->
    <template v-if="isEdit && productId">
      <Divider />
      <h6 class="font-bold mb-2">产品图片</h6>
      <div class="d-flex flex-wrap gap-2 mb-3">
        <div v-for="img in images" :key="img.id" class="border rounded p-1 text-center" style="width:110px">
          <img :src="'/' + img.path" style="height:80px;width:100%;object-fit:cover" alt="" class="rounded" />
          <Button label="删除" size="small" severity="danger" outlined class="mt-1 w-full" @click="deleteImage(img.id)" />
        </div>
      </div>
      <FileUpload mode="basic" name="files" accept="image/*" choose-label="上传图片" :custom-upload="true"
                  @uploader="uploadImages" />
    </template>

    <template #footer>
      <Button label="取消" severity="secondary" outlined @click="visible = false" />
      <Button label="保存" icon="pi pi-check" @click="save" :loading="saving" />
    </template>
  </Dialog>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import api from '../api'

const props = defineProps({ visible: Boolean, productId: { type: Number, default: null } })
const emit = defineEmits(['update:visible', 'saved'])

const toast = useToast()
const isEdit = ref(false)
const form = ref({ id: null, model: '', name: '', description: '', seriesId: null, categoryId: null, isActive: true })
const specs = ref([])
const images = ref([])
const series = ref([])
const categories = ref([])
const formError = ref('')
const saving = ref(false)

// 打开时加载字典与数据
watch(() => props.visible, async (v) => {
  if (!v) return
  formError.value = ''
  const [{ data: cat }, { data: ser }] = await Promise.all([api.get('/catalog'), api.get('/catalog')])
  series.value = ser.series
  categories.value = flattenCategories(cat.categoryTree)
  if (props.productId) {
    isEdit.value = true
    const { data } = await api.get(`/products/${props.productId}`)
    form.value = {
      id: data.id, model: data.model, name: data.name, description: data.description,
      seriesId: data.seriesId ?? null, categoryId: data.categoryId ?? null, isActive: data.isActive
    }
    specs.value = data.specs.map(s => ({ name: s.name, value: s.value }))
    images.value = data.images
  } else {
    isEdit.value = false
    form.value = { id: null, model: '', name: '', description: '', seriesId: null, categoryId: null, isActive: true }
    specs.value = []
    images.value = []
  }
})

function flattenCategories(nodes, prefix = '', out = []) {
  nodes.forEach(n => {
    const label = prefix ? `${prefix} / ${n.name}` : n.name
    out.push({ id: n.id, label })
    flattenCategories(n.children || [], label, out)
  })
  return out
}

async function save() {
  formError.value = ''
  if (!form.value.model.trim()) { formError.value = '型号不能为空'; return }
  saving.value = true
  try {
    const payload = {
      ...form.value,
      model: form.value.model.trim(),
      specNames: specs.value.map(s => s.name),
      specValues: specs.value.map(s => s.value)
    }
    if (isEdit.value) await api.put(`/products/${props.productId}`, payload)
    else await api.post('/products', payload)
    toast.add({ severity: 'success', summary: '已保存', life: 2000 })
    emit('saved')
    emit('update:visible', false)
  } catch (e) {
    formError.value = e.response?.data || e.message
  } finally {
    saving.value = false
  }
}

async function uploadImages(event) {
  const fd = new FormData()
  Array.from(event.files).forEach(f => fd.append('files', f))
  await api.post(`/products/${props.productId}/images`, fd)
  const { data } = await api.get(`/products/${props.productId}`)
  images.value = data.images
  toast.add({ severity: 'success', summary: '图片已上传', life: 2000 })
  emit('saved')
}

async function deleteImage(imageId) {
  await api.delete(`/products/${props.productId}/images/${imageId}`)
  images.value = images.value.filter(i => i.id !== imageId)
  toast.add({ severity: 'success', summary: '图片已删除', life: 2000 })
}
</script>
