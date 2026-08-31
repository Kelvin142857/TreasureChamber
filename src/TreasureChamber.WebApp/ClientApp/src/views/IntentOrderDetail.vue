<template>
  <div>
    <ProgressSpinner v-if="loading" style="width:48px;height:48px" class="d-block mx-auto my-5" />
    <template v-else-if="order">
      <Breadcrumb :home="{ icon: 'pi pi-home', command: () => router.push('/') }"
                  :model="[{ label: '意向单', command: () => router.push('/intent-orders') }, { label: order.orderNo }]" class="mb-3" />

      <Card class="mb-3">
        <template #title>
          <div class="d-flex justify-content-between align-items-center">
            <span>{{ order.orderNo }}</span>
            <Tag :value="order.statusLabel" :severity="statusSeverity(order.status)" />
          </div>
        </template>
        <template #content>
          <div class="grid">
            <div class="col-12 md:col-3"><div class="text-muted small">客户姓名</div><div class="font-bold">{{ order.customerName }}</div></div>
            <div class="col-12 md:col-3"><div class="text-muted small">联系电话</div><div class="font-bold">{{ order.customerPhone }}</div></div>
            <div class="col-12 md:col-3"><div class="text-muted small">公司 / 单位</div><div>{{ order.customerCompany || '-' }}</div></div>
            <div class="col-12 md:col-3"><div class="text-muted small">创建时间</div><div>{{ formatTime(order.createdAt) }}</div></div>
            <div v-if="order.note" class="col-12"><div class="text-muted small">备注</div><div style="white-space:pre-wrap">{{ order.note }}</div></div>
          </div>
          <Divider />
          <div class="d-flex gap-2 align-items-center">
            <span class="font-bold">更新状态：</span>
            <SelectButton v-model="editStatus" :options="statusOptions" option-label="label" option-value="value" />
            <Button label="保存" icon="pi pi-check" size="small" @click="updateStatus" :disabled="editStatus === order.status" />
          </div>
        </template>
      </Card>

      <DataTable :value="order.items" data-key="id" striped-rows class="p-datatable-sm">
        <Column field="productModel" header="产品型号">
          <template #body="{ data }"><span class="font-bold">{{ data.productModel }}</span></template>
        </Column>
        <Column field="productName" header="产品名称">
          <template #body="{ data }">
            {{ data.productName }}
            <Button v-if="data.productId" label="查看产品" text size="small" class="ml-2"
                    @click="router.push(`/product/${data.productId}`)" />
            <Tag v-else value="产品已删除" severity="secondary" class="ml-2" />
          </template>
        </Column>
        <Column field="quantity" header="数量" style="width:90px" />
        <Column field="remark" header="备注">
          <template #body="{ data }"><span class="text-muted">{{ data.remark || '-' }}</span></template>
        </Column>
      </DataTable>

      <div class="mt-3">
        <Button label="返回列表" severity="secondary" outlined @click="router.push('/intent-orders')" />
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import api from '../api'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const order = ref(null)
const loading = ref(true)
const editStatus = ref(0)

const statusOptions = [
  { label: '新建', value: 0 },
  { label: '跟进中', value: 1 },
  { label: '已成交', value: 2 },
  { label: '已放弃', value: 3 }
]

onMounted(async () => {
  try {
    const { data } = await api.get(`/intent-orders/${route.params.id}`)
    order.value = data
    editStatus.value = data.status
  } finally {
    loading.value = false
  }
})

function statusSeverity(s) {
  return { 0: 'info', 1: 'warning', 2: 'success', 3: 'secondary' }[s] || 'secondary'
}
function formatTime(iso) {
  const d = new Date(iso)
  const p = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())} ${p(d.getHours())}:${p(d.getMinutes())}`
}

async function updateStatus() {
  await api.put(`/intent-orders/${order.value.id}/status?status=${editStatus.value}`)
  order.value.status = editStatus.value
  order.value.statusLabel = statusOptions.find(s => s.value === editStatus.value).label
  toast.add({ severity: 'success', summary: '状态已更新', life: 2000 })
}
</script>
