<template>
  <div>
    <h4 class="font-bold mb-3">新建意向单</h4>
    <Message v-if="formError" severity="error" :closable="false" class="mb-3">{{ formError }}</Message>

    <!-- 客户信息 -->
    <Card class="mb-3">
      <template #title>客户信息</template>
      <template #content>
        <div class="grid">
          <div class="col-12 md:col-4">
            <label class="font-bold block mb-1">客户姓名 <span class="text-danger">*</span></label>
            <InputText v-model="form.customerName" class="w-full" />
          </div>
          <div class="col-12 md:col-4">
            <label class="font-bold block mb-1">联系电话 <span class="text-danger">*</span></label>
            <InputText v-model="form.customerPhone" class="w-full" />
          </div>
          <div class="col-12 md:col-4">
            <label class="font-bold block mb-1">公司 / 单位</label>
            <InputText v-model="form.customerCompany" class="w-full" />
          </div>
          <div class="col-12">
            <label class="font-bold block mb-1">备注</label>
            <Textarea v-model="form.note" rows="2" class="w-full" placeholder="客户需求、跟进事项等" />
          </div>
        </div>
      </template>
    </Card>

    <!-- 选择产品 -->
    <Card>
      <template #title>选择产品</template>
      <template #content>
        <div class="d-flex gap-2 mb-3" style="max-width: 480px;">
          <InputText v-model="pickerKeyword" placeholder="输入型号或名称搜索产品" class="flex-1" @keyup.enter="searchProducts" />
          <Button label="搜索" icon="pi pi-search" outlined @click="searchProducts" />
        </div>

        <Dialog v-model:visible="pickerVisible" header="选择产品" :style="{ width: '640px' }" modal>
          <DataTable :value="pickerResults" data-key="id" :rows="10" paginator class="p-datatable-sm">
            <Column field="model" header="型号">
              <template #body="{ data }"><span class="font-bold">{{ data.model }}</span></template>
            </Column>
            <Column field="name" header="名称" />
            <Column header="操作" style="width:90px">
              <template #body="{ data }">
                <Button label="添加" size="small" @click="addItem(data)" />
              </template>
            </Column>
          </DataTable>
        </Dialog>

        <DataTable :value="items" data-key="uid" class="p-datatable-sm mb-2">
          <Column header="产品">
            <template #body="{ data }">
              <div class="font-bold">{{ data.model }}</div>
              <div class="text-muted small">{{ data.name }}</div>
            </template>
          </Column>
          <Column header="数量" style="width:120px">
            <template #body="{ data }">
              <InputNumber v-model="data.quantity" :min="1" class="w-full" />
            </template>
          </Column>
          <Column header="备注" style="width:220px">
            <template #body="{ data }">
              <InputText v-model="data.remark" class="w-full" placeholder="可选" />
            </template>
          </Column>
          <Column header="" style="width:70px">
            <template #body="{ data }">
              <Button icon="pi pi-trash" text rounded severity="danger" @click="removeItem(data.uid)" />
            </template>
          </Column>
        </DataTable>
        <div v-if="!items.length" class="text-muted small">尚未添加产品，用上方搜索选择。</div>
      </template>
    </Card>

    <div class="mt-4">
      <Button label="保存意向单" icon="pi pi-check" severity="success" :loading="saving" @click="save" />
      <Button label="返回列表" severity="secondary" outlined class="ml-2" @click="router.push('/intent-orders')" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import api from '../api'

const router = useRouter()
const route = useRoute()
const toast = useToast()

const form = ref({ customerName: '', customerPhone: '', customerCompany: '', note: '' })
const items = ref([])
const formError = ref('')
const saving = ref(false)
let uidSeq = 0

const pickerKeyword = ref('')
const pickerVisible = ref(false)
const pickerResults = ref([])

onMounted(() => {
  const pid = Number(route.query.product)
  if (pid) {
    api.get(`/products/${pid}`).then(({ data }) => {
      items.value.push({ uid: ++uidSeq, productId: data.id, model: data.model, name: data.name, quantity: 1, remark: '' })
    })
  }
})

async function searchProducts() {
  const { data } = await api.get('/products/picker', { params: { keyword: pickerKeyword.value } })
  pickerResults.value = data
  pickerVisible.value = true
}

function addItem(p) {
  if (items.value.some(i => i.productId === p.id)) {
    toast.add({ severity: 'warn', summary: '该产品已在列表中', life: 2000 })
    return
  }
  items.value.push({ uid: ++uidSeq, productId: p.id, model: p.model, name: p.name, quantity: 1, remark: '' })
}
function removeItem(uid) {
  items.value = items.value.filter(i => i.uid !== uid)
}

async function save() {
  formError.value = ''
  if (!form.value.customerName.trim()) { formError.value = '请填写客户姓名'; return }
  if (!form.value.customerPhone.trim()) { formError.value = '请填写联系电话'; return }
  if (!items.value.length) { formError.value = '请至少添加一款产品'; return }

  saving.value = true
  try {
    const { data } = await api.post('/intent-orders', {
      customerName: form.value.customerName.trim(),
      customerPhone: form.value.customerPhone.trim(),
      customerCompany: form.value.customerCompany.trim() || null,
      note: form.value.note.trim() || null,
      items: items.value.map(i => ({
        productId: i.productId,
        model: i.model,
        quantity: i.quantity,
        remark: i.remark || null
      }))
    })
    toast.add({ severity: 'success', summary: '意向单已保存', life: 2000 })
    router.push(`/intent-orders/${data.id}`)
  } catch (e) {
    formError.value = e.response?.data || e.message
  } finally {
    saving.value = false
  }
}
</script>
